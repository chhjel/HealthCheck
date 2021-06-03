using Fido2NetLib;
using Fido2NetLib.Objects;
using HealthCheck.Core.Models;
using HealthCheck.WebUI.MFA.WebAuthn.Abstractions;
using HealthCheck.WebUI.MFA.WebAuthn.Extensions;
using HealthCheck.WebUI.MFA.WebAuthn.Models;
using HealthCheck.WebUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthCheck.WebUI.MFA.WebAuthn
{
    /// <summary>
    /// Helper methods for WebAuthn/FIDO2.
    /// </summary>
    public class HCWebAuthnHelper
	{
		private readonly Fido2 _lib;
		private readonly IHCWebAuthnCredentialManager _credentialManager;

		/// <summary>
		/// Helper methods for WebAuthn/FIDO2.
		/// </summary>
		/// <param name="serverDomain">Domain of the server the site is running on. For localhost use 'localhost'.</param>
		/// <param name="serverName">Display name shown in the browser popup when requesting access to the FIDO key.</param>
		/// <param name="origin">Origin from request headers. E.g. <c>Request.Headers["Origin"]</c></param>
		/// <param name="credentialManager">Implementation that stores webauthn credentials.</param>
		/// <param name="timestampDriftTolerance">Time in milliseconds that will be allowed for clock drift on a timestamped attestation.</param>
		public HCWebAuthnHelper(string serverDomain, string serverName, string origin, IHCWebAuthnCredentialManager credentialManager, int timestampDriftTolerance = 300000)
		{
			_lib = new Fido2(new Fido2Configuration()
			{
				ServerDomain = serverDomain,
				ServerName = serverName,
				Origin = origin,
                TimestampDriftTolerance = timestampDriftTolerance
			});
            _credentialManager = credentialManager;
        }

		/// <summary>
		/// To add FIDO2 credentials to an existing user account, we we perform a attestation process. It starts with returning options to the client.
		/// </summary>
		public CredentialCreateOptions CreateClientOptions(string username)
		{
			// 1. Get user from DB by username (in our example, auto create missing users)
			var user = _credentialManager.GetOrAddUser(username, () => new Fido2User
			{
				DisplayName = "Display " + username,
				Name = username,
				Id = Encoding.UTF8.GetBytes(username) // byte representation of userID is required
			});

			// 2. Get user existing keys by username
			List<PublicKeyCredentialDescriptor> existingKeys = _credentialManager.GetCredentialsByUser(user).Select(c => c.Descriptor).ToList();

			// 3. Create options
			var authenticatorSelection = new AuthenticatorSelection
			{
				RequireResidentKey = false,
				UserVerification = UserVerificationRequirement.Preferred
			};
			var exts = new AuthenticationExtensionsClientInputs()
			{
				Extensions = true,
				UserVerificationIndex = true,
				Location = true,
				UserVerificationMethod = true,
				BiometricAuthenticatorPerformanceBounds = new AuthenticatorBiometricPerfBounds
				{
					FAR = float.MaxValue,
					FRR = float.MaxValue
				}
			};

			var options = _lib.RequestNewCredential(user, existingKeys, authenticatorSelection, AttestationConveyancePreference.None, exts);
			return options;
		}

		/// <summary>
		/// When the client returns a response, we verify and register the credentials.
		/// </summary>
		public async Task RegisterCredentials(CredentialCreateOptions options, AuthenticatorAttestationRawResponse attestationResponse)
		{
            // 2. Create callback so that lib can verify credential id is unique to this user
            async Task<bool> callback(IsCredentialIdUniqueToUserParams args)
            {
                List<Fido2User> users = await _credentialManager.GetUsersByCredentialIdAsync(args.CredentialId);
                if (users.Count > 0) return false;

                return true;
            }

            // 2. Verify and make the credentials
            var success = await _lib.MakeNewCredentialAsync(attestationResponse, options, callback);

			// 3. Store the credentials in db
			_credentialManager.AddCredentialToUser(options.User, new HCWebAuthnStoredCredential
			{
				Descriptor = new PublicKeyCredentialDescriptor(success.Result.CredentialId),
				PublicKey = success.Result.PublicKey,
				UserHandle = success.Result.User.Id
			});
		}

		/// <summary>
		/// When a user wants to log a user in, we do an assertion based on the registered credentials.
		/// </summary>
		public AssertionOptions CreateAssertionOptions(string username)
		{
			// 1. Get user from DB
			var user = _credentialManager.GetUser(username);
			if (user == null) return null;

			// 2. Get registered credentials from database
			List<PublicKeyCredentialDescriptor> existingCredentials = _credentialManager.GetCredentialsByUser(user).Select(c => c.Descriptor).ToList();

			// 3. Create options
			return _lib.GetAssertionOptions(
				existingCredentials,
				UserVerificationRequirement.Discouraged
			);
		}

		/// <summary>
		/// When the client returns a response, we verify it and accepts the login.
		/// </summary>
		public async Task<HCGenericResult<AssertionVerificationResult>> VerifyAssertion(AssertionOptions options, VerifyWebAuthnAssertionModel attestationResponse)
			=> await VerifyAssertion(options, attestationResponse.ToAuthenticatorAssertionRawResponse());

		/// <summary>
		/// When the client returns a response, we verify it and accepts the login.
		/// </summary>
		public async Task<HCGenericResult<AssertionVerificationResult>> VerifyAssertion(AssertionOptions options, AuthenticatorAssertionRawResponse clientResponse)
		{
			// 2. Get registered credential from database
			HCWebAuthnStoredCredential creds = _credentialManager.GetCredentialById(clientResponse.Id);
			if (creds == null)
            {
				return HCGenericResult<AssertionVerificationResult>.CreateError("Invalid credentials.");
            }

			// 3. Get credential counter from database
			var storedCounter = creds.SignatureCounter;

            // 4. Create callback to check if userhandle owns the credentialId
            async Task<bool> callback(IsUserHandleOwnerOfCredentialIdParams args)
            {
                List<HCWebAuthnStoredCredential> storedCreds = await _credentialManager.GetCredentialsByUserHandleAsync(args.UserHandle);
                return storedCreds.Exists(c => c.Descriptor.Id.SequenceEqual(args.CredentialId));
            }

            // 5. Make the assertion
            var res = await _lib.MakeAssertionAsync(clientResponse, options, creds.PublicKey, storedCounter, callback);

			// 6. Store the updated counter
			_credentialManager.UpdateCounter(res.CredentialId, res.Counter);

			if (res?.Status != "ok")
            {
				return HCGenericResult<AssertionVerificationResult>.CreateError($"Verification failed. {res.ErrorMessage}");
			}

			return HCGenericResult<AssertionVerificationResult>.CreateSuccess(res);
		}

		/// <summary>
		/// Creates an assertion options json model from the given request.
		/// </summary>
		public string CreateWebAuthnAssertionOptionsJson(HCIntegratedLoginCreateWebAuthnAssertionOptionsRequest request)
		{
            var options = CreateAssertionOptions(request.Username);
			return options.ToJson();
        }
	}
}
