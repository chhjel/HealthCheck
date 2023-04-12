using Fido2NetLib.Objects;
using QoDL.Toolkit.WebUI.MFA.WebAuthn.Abstractions;
using System;

namespace QoDL.Toolkit.WebUI.MFA.WebAuthn.Models;

/// <summary>
/// Data model stored in <see cref="ITKWebAuthnCredentialManager"/>.
/// </summary>
public class TKWebAuthnStoredCredential
	{
		/// <summary></summary>
		public byte[] UserId { get; set; }
		/// <summary></summary>
		public PublicKeyCredentialDescriptor Descriptor { get; set; }
		/// <summary></summary>
		public byte[] PublicKey { get; set; }
		/// <summary></summary>
		public byte[] UserHandle { get; set; }
		/// <summary></summary>
		public uint SignatureCounter { get; set; }
		/// <summary></summary>
		public string CredType { get; set; }
		/// <summary></summary>
		public DateTime RegDate { get; set; }
		/// <summary></summary>
		public Guid AaGuid { get; set; }
	}
