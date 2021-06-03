using Fido2NetLib;
using HealthCheck.WebUI.MFA.WebAuthn.Abstractions;
using HealthCheck.WebUI.MFA.WebAuthn.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.WebUI.MFA.WebAuthn.Storage
{
    /// <summary>
    /// Simple implementation that only stores data statically in memory.
    /// </summary>
    public class HCMemoryWebAuthnCredentialManager : IHCWebAuthnCredentialManager
	{
		private static readonly ConcurrentDictionary<string, Fido2User> _storedUsers = new();
		private static readonly List<HCWebAuthnStoredCredential> _storedCredentials = new();

		/// <inheritdoc />
		public Fido2User GetOrAddUser(string username, Func<Fido2User> factory)
		{
			return _storedUsers.GetOrAdd(username, factory());
		}

		/// <inheritdoc />
		public Fido2User GetUser(string username)
		{
			_storedUsers.TryGetValue(username, out var user);
			return user;
		}

		/// <inheritdoc />
		public List<HCWebAuthnStoredCredential> GetCredentialsByUser(Fido2User user)
			=> _storedCredentials.Where(c => c.UserId.SequenceEqual(user.Id)).ToList();

		/// <inheritdoc />
		public HCWebAuthnStoredCredential GetCredentialById(byte[] id)
			=> _storedCredentials.FirstOrDefault(c => c.Descriptor.Id.SequenceEqual(id));

		/// <inheritdoc />
		public Task<List<HCWebAuthnStoredCredential>> GetCredentialsByUserHandleAsync(byte[] userHandle)
			=> Task.FromResult(_storedCredentials.Where(c => c.UserHandle.SequenceEqual(userHandle)).ToList());

		/// <inheritdoc />
		public void UpdateCounter(byte[] credentialId, uint counter)
		{
			var cred = _storedCredentials.FirstOrDefault(c => c.Descriptor.Id.SequenceEqual(credentialId));
			cred.SignatureCounter = counter;
		}

		/// <inheritdoc />
		public void AddCredentialToUser(Fido2User user, HCWebAuthnStoredCredential credential)
		{
			credential.UserId = user.Id;
			_storedCredentials.Add(credential);
		}

		/// <inheritdoc />
		public Task<List<Fido2User>> GetUsersByCredentialIdAsync(byte[] credentialId)
		{
			// our in-mem storage does not allow storing multiple users for a given credentialId. Yours shouldn't either.
			var cred = _storedCredentials.FirstOrDefault(c => c.Descriptor.Id.SequenceEqual(credentialId));

			if (cred == null)
				return Task.FromResult(new List<Fido2User>());

			return Task.FromResult(_storedUsers.Where(u => u.Value.Id.SequenceEqual(cred.UserId)).Select(u => u.Value).ToList());
		}
	}
}
