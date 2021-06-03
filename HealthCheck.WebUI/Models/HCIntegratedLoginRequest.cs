namespace HealthCheck.WebUI.Models
{
    /// <summary>
    /// Request sent to custom login handler.
    /// </summary>
    public class HCIntegratedLoginRequest
    {
        /// <summary>
        /// Entered username.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Entered password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Entered 2FA code.
        /// </summary>
        public string TwoFactorCode { get; set; }

        /// <summary>
        /// Payload from WebAuthn if any.
        /// </summary>
        public VerifyWebAuthnAssertionModel WebAuthnPayload { get; set; }
    }
}
