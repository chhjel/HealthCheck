using Fido2NetLib;
using Fido2NetLib.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthCheck.WebUI.MFA.FIDO2
{
	public class HCFido2Helper
	{
		private Fido2 _lib;
		private IFido2CredentialManager _store = new DevelopmentInMemoryStore();

		public HCFido2Helper(string serverDomain, string serverName, string origin)
		{
			_lib = new Fido2(new Fido2Configuration()
			{
				ServerDomain = serverDomain,
				ServerName = serverName,
				Origin = origin,
                TimestampDriftTolerance = 300000
            });
		}

		/// <summary>
		/// To add FIDO2 credentials to an existing user account, we we perform a attestation process. It starts with returning options to the client.
		/// </summary>
		public CredentialCreateOptions CreateClientOptions(string username)
		{
			// 1. Get user from DB by username (in our example, auto create missing users)
			var user = _store.GetOrAddUser(username, () => new Fido2User
			{
				DisplayName = "Display " + username,
				Name = username,
				Id = Encoding.UTF8.GetBytes(username) // byte representation of userID is required
			});

			// 2. Get user existing keys by username
			List<PublicKeyCredentialDescriptor> existingKeys = _store.GetCredentialsByUser(user).Select(c => c.Descriptor).ToList();

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

			// 4. Temporarily store options, session/in-memory cache/redis/db
			//HttpContext.Session.SetString("fido2.attestationOptions", options.ToJson());

			// 5. return options to client
			//return Json(options);
		}

		/// <summary>
		/// When the client returns a response, we verify and register the credentials.
		/// </summary>
		public async Task RegisterCredentials(CredentialCreateOptions options, AuthenticatorAttestationRawResponse attestationResponse)
		{
			// 2. Create callback so that lib can verify credential id is unique to this user
			IsCredentialIdUniqueToUserAsyncDelegate callback = async (IsCredentialIdUniqueToUserParams args) =>
			{
				List<Fido2User> users = await _store.GetUsersByCredentialIdAsync(args.CredentialId);
				if (users.Count > 0) return false;

				return true;
			};

			// 2. Verify and make the credentials
			var success = await _lib.MakeNewCredentialAsync(attestationResponse, options, callback);

			// 3. Store the credentials in db
			_store.AddCredentialToUser(options.User, new StoredCredential
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
			var user = _store.GetUser(username);
			if (user == null) return null;

			// 2. Get registered credentials from database
			List<PublicKeyCredentialDescriptor> existingCredentials = _store.GetCredentialsByUser(user).Select(c => c.Descriptor).ToList();

			// 3. Create options
			return _lib.GetAssertionOptions(
				existingCredentials,
				UserVerificationRequirement.Discouraged
			);
		}

		/// <summary>
		/// When the client returns a response, we verify it and accepts the login.
		/// </summary>
		public async Task<AssertionVerificationResult> VerifyAssertion(AssertionOptions options, AuthenticatorAssertionRawResponse clientResponse)
		{
			// 2. Get registered credential from database
			StoredCredential creds = _store.GetCredentialById(clientResponse.Id);
			if (creds == null)
            {
				return null;
            }

			// 3. Get credential counter from database
			var storedCounter = creds.SignatureCounter;

            // 4. Create callback to check if userhandle owns the credentialId
            async Task<bool> callback(IsUserHandleOwnerOfCredentialIdParams args)
            {
                List<StoredCredential> storedCreds = await _store.GetCredentialsByUserHandleAsync(args.UserHandle);
                return storedCreds.Exists(c => c.Descriptor.Id.SequenceEqual(args.CredentialId));
            }

            // 5. Make the assertion
            var res = await _lib.MakeAssertionAsync(clientResponse, options, creds.PublicKey, storedCounter, callback);

			// 6. Store the updated counter
			_store.UpdateCounter(res.CredentialId, res.Counter);

			return res;
		}
	}

	public interface IFido2CredentialStore
	{
		//Fido2User GetOrAddUser(string username, Func<Fido2User> factory);
		Fido2User GetUser(string username);
		List<StoredCredential> GetCredentialsByUser(Fido2User user);
		StoredCredential GetCredentialById(byte[] id);
		Task<List<StoredCredential>> GetCredentialsByUserHandleAsync(byte[] userHandle);
	}

	public interface IFido2CredentialManager : IFido2CredentialStore
	{
		Fido2User GetOrAddUser(string username, Func<Fido2User> factory);
		void UpdateCounter(byte[] credentialId, uint counter);
		void AddCredentialToUser(Fido2User user, StoredCredential credential);
		Task<List<Fido2User>> GetUsersByCredentialIdAsync(byte[] credentialId);
	}

	public class DevelopmentInMemoryStore : IFido2CredentialManager
	{
		private static readonly ConcurrentDictionary<string, Fido2User> _storedUsers = new();
		private static readonly List<StoredCredential> _storedCredentials = new();

		public Fido2User GetOrAddUser(string username, Func<Fido2User> factory)
		{
			return _storedUsers.GetOrAdd(username, factory());
		}

		public Fido2User GetUser(string username)
		{
			_storedUsers.TryGetValue(username, out var user);
			return user;
		}

		public List<StoredCredential> GetCredentialsByUser(Fido2User user)
			=> _storedCredentials.Where(c => c.UserId.SequenceEqual(user.Id)).ToList();

		public StoredCredential GetCredentialById(byte[] id)
			=> _storedCredentials.Where(c => c.Descriptor.Id.SequenceEqual(id)).FirstOrDefault();

		public Task<List<StoredCredential>> GetCredentialsByUserHandleAsync(byte[] userHandle)
			=> Task.FromResult(_storedCredentials.Where(c => c.UserHandle.SequenceEqual(userHandle)).ToList());

		public void UpdateCounter(byte[] credentialId, uint counter)
		{
			var cred = _storedCredentials.Where(c => c.Descriptor.Id.SequenceEqual(credentialId)).FirstOrDefault();
			cred.SignatureCounter = counter;
		}

		public void AddCredentialToUser(Fido2User user, StoredCredential credential)
		{
			credential.UserId = user.Id;
			_storedCredentials.Add(credential);
		}

		public Task<List<Fido2User>> GetUsersByCredentialIdAsync(byte[] credentialId)
		{
			// our in-mem storage does not allow storing multiple users for a given credentialId. Yours shouldn't either.
			var cred = _storedCredentials.Where(c => c.Descriptor.Id.SequenceEqual(credentialId)).FirstOrDefault();

			if (cred == null)
				return Task.FromResult(new List<Fido2User>());

			return Task.FromResult(_storedUsers.Where(u => u.Value.Id.SequenceEqual(cred.UserId)).Select(u => u.Value).ToList());
		}
	}

	public class StoredCredential
	{
		public byte[] UserId { get; set; }
		public PublicKeyCredentialDescriptor Descriptor { get; set; }
		public byte[] PublicKey { get; set; }
		public byte[] UserHandle { get; set; }
		public uint SignatureCounter { get; set; }
		public string CredType { get; set; }
		public DateTime RegDate { get; set; }
		public Guid AaGuid { get; set; }
	}
}
