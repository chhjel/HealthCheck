using Fido2NetLib;
using QoDL.Toolkit.WebUI.MFA.WebAuthn.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.WebUI.MFA.WebAuthn.Abstractions;

/// <summary>
/// Handles WebAuthn credential storage.
/// </summary>
public interface ITKWebAuthnCredentialManager
	{
		/// <summary></summary>
		Fido2User GetOrAddUser(string username, Func<Fido2User> factory);
		/// <summary></summary>
		void UpdateCounter(byte[] credentialId, uint counter);
		/// <summary></summary>
		void AddCredentialToUser(Fido2User user, TKWebAuthnStoredCredential credential);
		/// <summary></summary>
		Task<List<Fido2User>> GetUsersByCredentialIdAsync(byte[] credentialId);
		/// <summary></summary>
		Fido2User GetUser(string username);
		/// <summary></summary>
		List<TKWebAuthnStoredCredential> GetCredentialsByUser(Fido2User user);
		/// <summary></summary>
		TKWebAuthnStoredCredential GetCredentialById(byte[] id);
		/// <summary></summary>
		Task<List<TKWebAuthnStoredCredential>> GetCredentialsByUserHandleAsync(byte[] userHandle);
	}
