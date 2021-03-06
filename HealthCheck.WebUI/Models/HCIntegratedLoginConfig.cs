namespace HealthCheck.WebUI.Models
{
    /// <summary>
    /// Config for optional integrated login dialog.
    /// </summary>
    public class HCIntegratedLoginConfig
    {
        /// <summary>
        /// Login dialog will invoke this endpoint.
        /// <para>Should point to the <c>Login</c> action on a controller inheriting from <c>HealthCheckLoginControllerBase</c> where you can define the login logic.</para>
        /// </summary>
        public string IntegratedLoginEndpoint { get; set; }

        /// <summary>
        /// If enabled, 2FA input will be visible in the login dialog.
        /// </summary>
        public bool Show2FAInput { get; set; }
    }
}
