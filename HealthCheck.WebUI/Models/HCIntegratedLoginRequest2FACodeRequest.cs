namespace HealthCheck.WebUI.Models
{
    /// <summary>
    /// Request sent to custom login handler.
    /// </summary>
    public class HCIntegratedLoginRequest2FACodeRequest
    {
        /// <summary>
        /// Username.
        /// </summary>
        public string Username { get; set; }
    }
}
