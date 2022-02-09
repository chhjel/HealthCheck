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
        /// Config for optional integrated login dialog.
        /// </summary>
        /// <param name="integratedLoginEndpoint">
        /// Login dialog will invoke this endpoint.
        /// <para>Should point to the <c>Login</c> action on a controller inheriting from <c>HealthCheckLoginControllerBase</c> where you can define the login logic.</para>
        /// </param>
        public HCIntegratedLoginConfig(string integratedLoginEndpoint)
        {
            IntegratedLoginEndpoint = integratedLoginEndpoint;
        }

        /// <summary>
        /// Config for enabling two factor code input.
        /// </summary>
        public HCLoginTwoFactorCodeConfig TwoFactorCodeConfig { get; set; }

        /// <summary>
        /// Config for enabling WebAuthn two factor.
        /// </summary>
        public HCLoginWebAuthnConfig WebAuthnConfig { get; set; }

        /// <summary>
        /// Enable WebAuthn.
        /// </summary>
        public HCIntegratedLoginConfig EnableWebAuthn(bool required = true)
        {
            WebAuthnConfig = new HCLoginWebAuthnConfig
            {
                WebAuthnMode = required ? HCLoginWebAuthnMode.Required : HCLoginWebAuthnMode.Optional,
            };
            return this;
        }

        /// <summary>
        /// Enable time based one time password with an extra input field.
        /// </summary>
        /// <param name="current2FACodeExpirationTime">Optionally set to show a progressbar for when codes expire.</param>
        /// <param name="twoFactorCodeLifetime">Used when current2FACodeExpirationTime is also set to display when the codes expire.</param>
        /// <param name="required">If false, the input won't be required in the frontend.</param>
        public HCIntegratedLoginConfig EnableTimeBasedOneTimePassword(DateTimeOffset? current2FACodeExpirationTime = null, int twoFactorCodeLifetime = 30, bool required = true)
        {
            TwoFactorCodeConfig = new HCLoginTwoFactorCodeConfig
            {
                TwoFactorCodeInputMode = required ? HCLoginTwoFactorCodeInputMode.Required : HCLoginTwoFactorCodeInputMode.Optional,
                Current2FACodeExpirationTime = current2FACodeExpirationTime,
                TwoFactorCodeLifetime = twoFactorCodeLifetime
            };
            return this;
        }

        /// <summary>
        /// Enable one time password with an extra input field and a 'Send code' button.
        /// </summary>
        /// <param name="requestCodeEndpoint">
        /// Requests to send new 2FA codes will be sent to this endpoint.
        /// <para>Should point to the <c>Request2FACode</c> action on a controller inheriting from <c>HealthCheckLoginControllerBase</c> where you can define the logic.</para>
        /// </param>
        /// <param name="requestCodeButtonText">Text on button used to get code.</param>
        /// <param name="required">If false, the input won't be required in the frontend.</param>
        public HCIntegratedLoginConfig EnableOneTimePasswordWithCodeRequest(string requestCodeEndpoint, string requestCodeButtonText = "Send code", bool required = true)
        {
            TwoFactorCodeConfig = new HCLoginTwoFactorCodeConfig
            {
                TwoFactorCodeInputMode = required ? HCLoginTwoFactorCodeInputMode.Required : HCLoginTwoFactorCodeInputMode.Optional,
                Send2FACodeEndpoint = requestCodeEndpoint,
                Send2FACodeButtonText = requestCodeButtonText
            };
            return this;
        }

        /// <summary>
        /// Config for WebAuthn two factor.
        /// </summary>
        public class HCLoginWebAuthnConfig
        {
            /// <summary>
            /// How to display WebAuthn input.
            /// </summary>
            public HCLoginWebAuthnMode WebAuthnMode { get; set; }
        }

        /// <summary>
        /// Config for two factor code input.
        /// </summary>
        public class HCLoginTwoFactorCodeConfig
        {
            /// <summary>
            /// Requests to send new 2FA codes will be sent to this endpoint.
            /// <para>Setting this value results in the button to send codes being shown.</para>
            /// <para>Should point to the <c>Request2FACode</c> action on a controller inheriting from <c>HealthCheckLoginControllerBase</c> where you can define the logic.</para>
            /// </summary>
            public string Send2FACodeEndpoint { get; set; }

            /// <summary>
            /// Text on button used to get code.
            /// <para>Defaults to 'Send code'</para>
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

            /// <summary>
            /// How to display two factor code input.
            /// </summary>
            public HCLoginTwoFactorCodeInputMode TwoFactorCodeInputMode { get; set; }
        }

        /// <summary>
        /// WebAuthn login mode.
        /// </summary>
        public enum HCLoginWebAuthnMode
        {
            /// <summary>
            /// Hide option to use WebAuthn login.
            /// </summary>
            Off = 0,

            /// <summary>
            /// Show WebAuthn button but do not require it to submit the form.
            /// </summary>
            Optional = 1,

            /// <summary>
            /// Require WebAuthn in order to login.
            /// </summary>
            Required = 2
        }

        /// <summary>
        /// Two factor code input mode.
        /// </summary>
        public enum HCLoginTwoFactorCodeInputMode
        {
            /// <summary>
            /// Hide 2FA code input.
            /// </summary>
            Off = 0,

            /// <summary>
            /// Do not require any 2FA code input.
            /// </summary>
            Optional = 1,

            /// <summary>
            /// Require 2FA code input.
            /// </summary>
            Required = 2
        }
    }
}
