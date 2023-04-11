using Fido2NetLib;
using Fido2NetLib.Objects;
using QoDL.Toolkit.Core.Models;
using QoDL.Toolkit.WebUI.MFA.WebAuthn.Abstractions;
using QoDL.Toolkit.WebUI.MFA.WebAuthn.Extensions;
using QoDL.Toolkit.WebUI.MFA.WebAuthn.Models;
using QoDL.Toolkit.WebUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QoDL.Toolkit.WebUI.MFA.WebAuthn
{
    /// <summary>
    /// Helper methods for WebAuthn/FIDO2.
    /// </summary>
    public class TKWebAuthnHelper
	{
		private readonly Fido2 _lib;
		private readonly ITKWebAuthnCredentialManager _credentialManager;

        /// <summary>
        /// Helper methods for WebAuthn/FIDO2.
        /// </summary>
        /// <param name="options">Options passed to Fido2.</param>
        /// <param name="credentialManager">Implementation that stores webauthn credentials.</param>
        public TKWebAuthnHelper(TKWebAuthnHelperOptions options, ITKWebAuthnCredentialManager credentialManager)
		{
			if (options == null) throw new ArgumentNullException(nameof(options));
			options.Validate();

			_lib = new Fido2(new Fido2Configuration()
			{
				ServerDomain = options.ServerDomain,
				ServerName = options.ServerName,
				Origin = options.Origin,
                TimestampDriftTolerance = (int)options.TimestampDriftTolerance.TotalMilliseconds
			});
            _credentialManager = credentialManager;
        }

		/// <summary>
		/// To add FIDO2 credentials to an existing user account, we we perform a attestation process. It starts with returning options to the client.
		/// </summary>
		public CredentialCreateOptions CreateClientOptions(string username, string displayName = null,
			Action<AuthenticationExtensionsClientInputs> extentsionsConfig = null,
			Action<AuthenticatorSelection> authSelectionConfig = null)
		{
			// 1. Get user from DB by username (in our example, auto create missing users)
			var user = _credentialManager.GetOrAddUser(username, () => new Fido2User
			{
				DisplayName = displayName ?? username,
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
			authSelectionConfig?.Invoke(authenticatorSelection);

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
			extentsionsConfig?.Invoke(exts);

			var options = _lib.RequestNewCredential(user, existingKeys, authenticatorSelection, AttestationConveyancePreference.None, exts);
			return options;
		}

		/// <summary>
		/// When the client returns a response, we verify and register the credentials.
		/// </summary>
		public async Task RegisterCredentials(CredentialCreateOptions options, TKRegisterWebAuthnModel attestation)
			=> await RegisterCredentials(options, attestation.ToAuthenticatorAttestationRawResponse());

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
			_credentialManager.AddCredentialToUser(options.User, new TKWebAuthnStoredCredential
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
		public async Task<TKGenericResult<AssertionVerificationResult>> VerifyAssertion(AssertionOptions options, TKVerifyWebAuthnAssertionModel attestationResponse)
			=> await VerifyAssertion(options, attestationResponse.ToAuthenticatorAssertionRawResponse());

		/// <summary>
		/// When the client returns a response, we verify it and accepts the login.
		/// </summary>
		public async Task<TKGenericResult<AssertionVerificationResult>> VerifyAssertion(AssertionOptions options, AuthenticatorAssertionRawResponse clientResponse)
		{
			// 2. Get registered credential from database
			TKWebAuthnStoredCredential creds = _credentialManager.GetCredentialById(clientResponse.Id);
			if (creds == null)
            {
				return TKGenericResult<AssertionVerificationResult>.CreateError("Invalid credentials.");
            }

			// 3. Get credential counter from database
			var storedCounter = creds.SignatureCounter;

            // 4. Create callback to check if userhandle owns the credentialId
            async Task<bool> callback(IsUserHandleOwnerOfCredentialIdParams args)
            {
                List<TKWebAuthnStoredCredential> storedCreds = await _credentialManager.GetCredentialsByUserHandleAsync(args.UserHandle);
                return storedCreds.Exists(c => c.Descriptor.Id.SequenceEqual(args.CredentialId));
            }

            // 5. Make the assertion
            var res = await _lib.MakeAssertionAsync(clientResponse, options, creds.PublicKey, storedCounter, callback);

			// 6. Store the updated counter
			_credentialManager.UpdateCounter(res.CredentialId, res.Counter);

			if (res?.Status != "ok")
            {
				return TKGenericResult<AssertionVerificationResult>.CreateError($"Verification failed. {res.ErrorMessage}");
			}

			return TKGenericResult<AssertionVerificationResult>.CreateSuccess(res);
		}

		/// <summary>
		/// Creates an assertion options json model from the given request.
		/// </summary>
		public string CreateWebAuthnAssertionOptionsJson(TKIntegratedLoginCreateWebAuthnAssertionOptionsRequest request)
		{
            var options = CreateAssertionOptions(request.Username);
			return options.ToJson();
        }
    }
}
