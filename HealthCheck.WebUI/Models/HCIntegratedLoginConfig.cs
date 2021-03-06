using System;

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

        /// <summary>
        /// Requests to send new 2FA codes will be sent to this endpoint.
        /// <para>Setting this value results in the button to send codes being shown.</para>
        /// <para>Should point to the <c>Request2FACode</c> action on a controller inheriting from <c>HealthCheckLoginControllerBase</c> where you can define the logic.</para>
        /// </summary>
        public string Send2FACodeEndpoint { get; set; }

        /// <summary>
        /// Text on button used to get code.
        /// <para>Defaults to 'Send 2FA code'</para>
        /// </summary>
        public string Send2FACodeButtonText { get; set; } = "Send code";

        /// <summary>
        /// Optionally set to show a progressbar for when codes expire.
        /// </summary>
        public DateTimeOffset? Current2FACodeExpirationTime { get; set; }

        /// <summary>
        /// Defaults to 30 seconds. Used when <see cref="Current2FACodeExpirationTime"/> is also set to display when the codes expire.
        /// </summary>
        public int TwoFactorCodeLifetime { get; set; } = 30;
    }
}
