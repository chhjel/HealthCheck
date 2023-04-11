using Fido2NetLib;
using QoDL.Toolkit.WebUI.MFA.WebAuthn.Abstractions;
using QoDL.Toolkit.WebUI.MFA.WebAuthn.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.WebUI.MFA.WebAuthn.Storage;

/// <summary>
/// Simple implementation that only stores data statically in memory.
/// </summary>
public class TKMemoryWebAuthnCredentialManager : ITKWebAuthnCredentialManager
	{
		private static readonly ConcurrentDictionary<string, Fido2User> _storedUsers = new();
		private static readonly List<TKWebAuthnStoredCredential> _storedCredentials = new();

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
		public List<TKWebAuthnStoredCredential> GetCredentialsByUser(Fido2User user)
			=> _storedCredentials.Where(c => c.UserId.SequenceEqual(user.Id)).ToList();

		/// <inheritdoc />
		public TKWebAuthnStoredCredential GetCredentialById(byte[] id)
			=> _storedCredentials.FirstOrDefault(c => c.Descriptor.Id.SequenceEqual(id));

		/// <inheritdoc />
		public Task<List<TKWebAuthnStoredCredential>> GetCredentialsByUserHandleAsync(byte[] userHandle)
			=> Task.FromResult(_storedCredentials.Where(c => c.UserHandle.SequenceEqual(userHandle)).ToList());

		/// <inheritdoc />
		public void UpdateCounter(byte[] credentialId, uint counter)
		{
			var cred = _storedCredentials.FirstOrDefault(c => c.Descriptor.Id.SequenceEqual(credentialId));
			cred.SignatureCounter = counter;
		}

		/// <inheritdoc />
		public void AddCredentialToUser(Fido2User user, TKWebAuthnStoredCredential credential)
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
