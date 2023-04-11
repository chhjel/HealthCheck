using System;

namespace QoDL.Toolkit.WebUI.Models;

/// <summary>
/// Config for optional integrated login dialog.
/// </summary>
public class TKIntegratedLoginConfig
{
    /// <summary>
    /// Login dialog will invoke this endpoint.
    /// <para>Should point to the <c>Login</c> action on a controller inheriting from <c>ToolkitLoginControllerBase</c> where you can define the login logic.</para>
    /// </summary>
    public string IntegratedLoginEndpoint { get; set; }

    /// <summary>
    /// Config for optional integrated login dialog.
    /// </summary>
    /// <param name="integratedLoginEndpoint">
    /// Login dialog will invoke this endpoint.
    /// <para>Should point to the <c>Login</c> action on a controller inheriting from <c>ToolkitLoginControllerBase</c> where you can define the login logic.</para>
    /// </param>
    public TKIntegratedLoginConfig(string integratedLoginEndpoint)
    {
        IntegratedLoginEndpoint = integratedLoginEndpoint;
    }

    /// <summary>
    /// Config for enabling two factor code input.
    /// </summary>
    public TKLoginTwoFactorCodeConfig TwoFactorCodeConfig { get; set; }

    /// <summary>
    /// Config for enabling WebAuthn two factor.
    /// </summary>
    public TKLoginWebAuthnConfig WebAuthnConfig { get; set; }

    /// <summary>
    /// Enable WebAuthn.
    /// <param name="note">Optional note below the the verify webauthn button.</param>
    /// <param name="required">If false, the input won't be required in the frontend.</param>
    /// </summary>
    public TKIntegratedLoginConfig EnableWebAuthn(string note = null, bool required = true)
    {
        WebAuthnConfig = new TKLoginWebAuthnConfig
        {
            WebAuthnMode = required ? TKLoginWebAuthnMode.Required : TKLoginWebAuthnMode.Optional,
            Note = note
        };
        return this;
    }

    /// <summary>
    /// Enable time based one time password with an extra input field.
    /// </summary>
    /// <param name="current2FACodeExpirationTime">Optionally set to show a progressbar for when codes expire.</param>
    /// <param name="twoFactorCodeLifetime">Used when current2FACodeExpirationTime is also set to display when the codes expire.</param>
    /// <param name="note">Optional note below the input field.</param>
    /// <param name="required">If false, the input won't be required in the frontend.</param>
    public TKIntegratedLoginConfig EnableTimeBasedOneTimePassword(DateTimeOffset? current2FACodeExpirationTime = null, int twoFactorCodeLifetime = 30, string note = null, bool required = true)
    {
        TwoFactorCodeConfig = new TKLoginTwoFactorCodeConfig
        {
            TwoFactorCodeInputMode = required ? TKLoginTwoFactorCodeInputMode.Required : TKLoginTwoFactorCodeInputMode.Optional,
            Current2FACodeExpirationTime = current2FACodeExpirationTime,
            TwoFactorCodeLifetime = twoFactorCodeLifetime,
            Note = note
        };
        return this;
    }

    /// <summary>
    /// Enable one time password with an extra input field and a 'Send code' button.
    /// </summary>
    /// <param name="requestCodeEndpoint">
    /// Requests to send new 2FA codes will be sent to this endpoint.
    /// <para>Should point to the <c>Request2FACode</c> action on a controller inheriting from <c>ToolkitLoginControllerBase</c> where you can define the logic.</para>
    /// </param>
    /// <param name="requestCodeButtonText">Text on button used to get code.</param>
    /// <param name="note">Optional note below the input field.</param>
    /// <param name="required">If false, the input won't be required in the frontend.</param>
    public TKIntegratedLoginConfig EnableOneTimePasswordWithCodeRequest(string requestCodeEndpoint, string requestCodeButtonText = "Send code", string note = null, bool required = true)
    {
        TwoFactorCodeConfig = new TKLoginTwoFactorCodeConfig
        {
            TwoFactorCodeInputMode = required ? TKLoginTwoFactorCodeInputMode.Required : TKLoginTwoFactorCodeInputMode.Optional,
            Send2FACodeEndpoint = requestCodeEndpoint,
            Send2FACodeButtonText = requestCodeButtonText,
            Note = note
        };
        return this;
    }

    /// <summary>
    /// Config for WebAuthn two factor.
    /// </summary>
    public class TKLoginWebAuthnConfig
    {
        /// <summary>
        /// How to display WebAuthn input.
        /// </summary>
        public TKLoginWebAuthnMode WebAuthnMode { get; set; }

        /// <summary>
        /// Optional note below the verify webauthn button.
        /// </summary>
        public string Note { get; set; }
    }

    /// <summary>
    /// Config for two factor code input.
    /// </summary>
    public class TKLoginTwoFactorCodeConfig
    {
        /// <summary>
        /// Requests to send new 2FA codes will be sent to this endpoint.
        /// <para>Setting this value results in the button to send codes being shown.</para>
        /// <para>Should point to the <c>Request2FACode</c> action on a controller inheriting from <c>ToolkitLoginControllerBase</c> where you can define the logic.</para>
        /// </summary>
        public string Send2FACodeEndpoint { get; set; }

        /// <summary>
        /// Text on button used to get code.
        /// <para>Defaults to 'Send code'</para>
        /// </summary>
        public string Send2FACodeButtonText { get; set; } = "Send code";

        /// <summary>
        /// Optional note below the input field.
        /// </summary>
        public string Note { get; set; }

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
        public TKLoginTwoFactorCodeInputMode TwoFactorCodeInputMode { get; set; }
    }

    /// <summary>
    /// WebAuthn login mode.
    /// </summary>
    public enum TKLoginWebAuthnMode
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
    public enum TKLoginTwoFactorCodeInputMode
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
