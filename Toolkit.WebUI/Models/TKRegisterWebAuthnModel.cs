using static QoDL.Toolkit.WebUI.Models.TKVerifyWebAuthnAssertionModel;

namespace QoDL.Toolkit.WebUI.Models
{
    /// <summary>
    /// Payload sent to ProfileRegisterWebAuthn.
    /// </summary>
    public class TKRegisterWebAuthnModel
    {
        /// <summary></summary>
        public string Id { get; set; }
        /// <summary></summary>
        public string RawId { get; set; }
        /// <summary></summary>
        public TKResponseData Response { get; set; }
        /// <summary></summary>
        public TKAuthenticationExtensionsClientOutputs Extensions { get; set; }

        /// <summary></summary>
        public class TKResponseData
        {
            /// <summary></summary>
            public string AttestationObject { get; set; }
            /// <summary></summary>
            public string ClientDataJson { get; set; }
        }
    }
}
