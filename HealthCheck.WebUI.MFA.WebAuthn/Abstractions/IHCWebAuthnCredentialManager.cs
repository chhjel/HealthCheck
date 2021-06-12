using Fido2NetLib;
using HealthCheck.WebUI.MFA.WebAuthn.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.WebUI.MFA.WebAuthn.Abstractions
{
    /// <summary>
    /// Handles WebAuthn credential storage.
    /// </summary>
    public interface IHCWebAuthnCredentialManager
	{
		/// <summary></summary>
		Fido2User GetOrAddUser(string username, Func<Fido2User> factory);
		/// <summary></summary>
		void UpdateCounter(byte[] credentialId, uint counter);
		/// <summary></summary>
		void AddCredentialToUser(Fido2User user, HCWebAuthnStoredCredential credential);
		/// <summary></summary>
		Task<List<Fido2User>> GetUsersByCredentialIdAsync(byte[] credentialId);
		/// <summary></summary>
		Fido2User GetUser(string username);
		/// <summary></summary>
		List<HCWebAuthnStoredCredential> GetCredentialsByUser(Fido2User user);
		/// <summary></summary>
		HCWebAuthnStoredCredential GetCredentialById(byte[] id);
		/// <summary></summary>
		Task<List<HCWebAuthnStoredCredential>> GetCredentialsByUserHandleAsync(byte[] userHandle);
	}
}
