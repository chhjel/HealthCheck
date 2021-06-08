using static HealthCheck.WebUI.Models.HCVerifyWebAuthnAssertionModel;

namespace HealthCheck.WebUI.Models
{
    /// <summary>
    /// Payload sent to ProfileRegisterWebAuthn.
    /// </summary>
    public class HCRegisterWebAuthnModel
    {
        /// <summary></summary>
        public string Id { get; set; }
        /// <summary></summary>
        public string RawId { get; set; }
        /// <summary></summary>
        public HCResponseData Response { get; set; }
        /// <summary></summary>
        public HCAuthenticationExtensionsClientOutputs Extensions { get; set; }

        /// <summary></summary>
        public class HCResponseData
        {
            /// <summary></summary>
            public string AttestationObject { get; set; }
            /// <summary></summary>
            public string ClientDataJson { get; set; }
        }
    }
}
