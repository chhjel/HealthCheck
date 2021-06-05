namespace HealthCheck.WebUI.Models
{
    /// <summary>
    /// WebAuthn login payload model.
    /// </summary>
    public class HCVerifyWebAuthnAssertionModel
    {
        /// <summary></summary>
        public string Id { get; set; }
        /// <summary></summary>
        public string RawId { get; set; }
        /// <summary></summary>
        public AssertionResponse Response { get; set; }
        /// <summary></summary>
        public AuthenticationExtensionsClientOutputs Extensions { get; set; }

        /// <summary>
        /// Part of WebAuthn login payload model.
        /// </summary>
        public class AssertionResponse
        {
            /// <summary></summary>
            public string AuthenticatorData { get; set; }
            /// <summary></summary>
            public string Signature { get; set; }
            /// <summary></summary>
            public string ClientDataJson { get; set; }
            // public string UserHandle
        }

        /// <summary>
        /// Part of WebAuthn login payload model.
        /// </summary>
        public class AuthenticationExtensionsClientOutputs
        {
            /// <summary></summary>
            public object Example { get; set; }
            /// <summary></summary>
            public bool AppID { get; set; }
            /// <summary></summary>
            public string txAuthSimple { get; set; }
            /// <summary></summary>
            public byte[] txAuthGenericArg { get; set; }
            /// <summary></summary>
            public bool authnSel { get; set; }
            /// <summary></summary>
            public string[] exts { get; set; }
            /// <summary></summary>
            public byte[] uvi { get; set; }
            /// <summary></summary>
            public AuthenticationGeoCoordinate loc { get; set; }
            /// <summary></summary>
            public ulong[][] uvm { get; set; }
            /// <summary></summary>
            public bool biometricPerfBounds { get; set; }
        }

        /// <summary>
        /// Part of WebAuthn login payload model.
        /// </summary>
        public class AuthenticationGeoCoordinate
        {
            /// <summary></summary>
            public double Course { get; set; }
            /// <summary></summary>
            public double Speed { get; set; }
            /// <summary></summary>
            public double VerticalAccuracy { get; set; }
            /// <summary></summary>
            public double HorizontalAccuracy { get; set; }
            /// <summary></summary>
            public double Longitude { get; set; }
            /// <summary></summary>
            public double Latitude { get; set; }
            /// <summary></summary>
            public double Altitude { get; set; }
            /// <summary></summary>
            public bool IsUnknown { get; }
        }
    }
}
