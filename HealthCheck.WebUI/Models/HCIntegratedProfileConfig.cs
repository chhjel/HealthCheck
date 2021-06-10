using HealthCheck.Core.Models;
using Newtonsoft.Json;

namespace HealthCheck.WebUI.Models
{
    /// <summary>
    /// Config for optional integrated profile.
    /// </summary>
    public class HCIntegratedProfileConfig
    {
        /// <summary>
        /// If set to true, the profile is disabled.
        /// </summary>
        public bool Hide { get; set; }

        /// <summary>
        /// Show resolved healthcheck roles for the user.
        /// <para>Defaults to true.</para>
        /// </summary>
        public bool ShowHealthCheckRoles { get; set; } = true;

        /// <summary>
        /// Displayed username, also used for WebAuthn elevation and registration.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Any extra html to display on the profile.
        /// </summary>
        public string BodyHtml { get; set; }

        /// <summary>
        /// If set, allows entering TOTP code from the profile to elevate access.
        /// <para>Input string is the one-time code.</para>
        /// </summary>
        [JsonIgnore]
        public ElevateTotpDelegate TotpElevationLogic { get; set; }
        /// <summary>Signature for elevating Totp.</summary>
        public delegate HCGenericResult<HCResultPageAction> ElevateTotpDelegate(string totpCode);
        /// <summary>
        /// Allows entering TOTP code from the profile to elevate access, requires <see cref="TotpElevationLogic"/> to be set.
        /// <para>Defaults to true.</para>
        /// </summary>
        public bool ShowTotpElevation { get; set; } = true;
        [JsonProperty] internal bool TotpElevationEnabled => ShowTotpElevation && TotpElevationLogic != null;

        /// <summary>
        /// If set, allows registering the users TOTP binding from the profile page.
        /// <para>Input string 1 is the users password to validate.</para>
        /// <para>Input string 2 is the TOTP secret to store on the user.</para>
        /// <para>Input string 3 is a code generated using the secret to be validated.</para>
        /// </summary>
        [JsonIgnore]
        public AddTotpDelegate AddTotpLogic { get; set; }
        /// <summary>Signature for adding Totp.</summary>
        public delegate HCGenericResult AddTotpDelegate(string password, string totpSecret, string totpCode);
        /// <summary>
        /// Allows registering the users TOTP binding from the profile page, requires <see cref="AddTotpLogic"/> to be set.
        /// <para>Defaults to true.</para>
        /// </summary>
        public bool ShowAddTotp { get; set; } = true;
        [JsonProperty] internal bool AddTotpEnabled => ShowAddTotp && AddTotpLogic != null;

        /// <summary>
        /// If set, allows removing the users TOTP binding from the profile page.
        /// <para>Input string is the users password to validate.</para>
        /// </summary>
        [JsonIgnore]
        public RemoveTotpDelegate RemoveTotpLogic { get; set; }
        /// <summary>Signature for removing TOTP.</summary>
        public delegate HCGenericResult RemoveTotpDelegate(string password);
        /// <summary>
        /// Allows removing the users TOTP binding from the profile page, requires <see cref="RemoveTotpLogic"/> to be set.
        /// <para>Defaults to true.</para>
        /// </summary>
        public bool ShowRemoveTotp { get; set; } = true;
        [JsonProperty] internal bool RemoveTotpEnabled => ShowRemoveTotp && RemoveTotpLogic != null;

        /// <summary>
        /// If set, allows authenticating using WebAuthn from the profile to elevate access.
        /// <para>Requires <see cref="Username"/> to be set.</para>
        /// <para>Input string is the webauthn payload.</para>
        /// <para>Also requires <see cref="CreateWebAuthnAssertionOptionsLogic"/> to be set.</para>
        /// </summary>
        [JsonIgnore]
        public ElevateWebAuthnDelegate WebAuthnElevationLogic { get; set; }
        /// <summary>Signature for elevating WebAuthn.</summary>
        public delegate HCGenericResult<HCResultPageAction> ElevateWebAuthnDelegate(HCVerifyWebAuthnAssertionModel payload);
        /// <summary>
        /// Allows authenticating using WebAuthn from the profile to elevate access, requires <see cref="WebAuthnElevationLogic"/> to be set.
        /// <para>Defaults to true.</para>
        /// </summary>
        public bool ShowWebAuthnElevation { get; set; } = true;
        [JsonProperty] internal bool WebAuthnElevationEnabled => ShowWebAuthnElevation 
            && WebAuthnElevationLogic != null && CreateWebAuthnAssertionOptionsLogic != null
            && !string.IsNullOrWhiteSpace(Username);

        /// <summary>
        /// Required for <see cref="WebAuthnElevationLogic"/> to function.
        /// <para>Should return the webauthn assertion options object.</para>
        /// </summary>
        [JsonIgnore]
        public CreateWebAuthnAssertionOptionsDelegate CreateWebAuthnAssertionOptionsLogic { get; set; }
        /// <summary>Signature for creating the webauthn assertion options object.</summary>
        public delegate HCGenericResult<object> CreateWebAuthnAssertionOptionsDelegate(string username);

        /// <summary>
        /// If set, allows registering the users WebAuthn binding from the profile page.
        /// <para>Requires <see cref="Username"/> to be set.</para>
        /// <para>Input string 1 is the users password to validate.</para>
        /// <para>Input string 2 is the WebAuthn payload in json format to store on the user.</para>
        /// <para>Also requires <see cref="CreateWebAuthnRegistrationOptionsLogic"/> to be set.</para>
        /// </summary>
        [JsonIgnore]
        public AddWebAuthnDelegate AddWebAuthnLogic { get; set; }
        /// <summary>Signature for adding WebAuthn.</summary>
        public delegate HCGenericResult AddWebAuthnDelegate(string password, HCRegisterWebAuthnModel webAuthnPayload);
        /// <summary>
        /// Allows registering the users WebAuthn binding from the profile page, requires <see cref="AddWebAuthnLogic"/> to be set.
        /// <para>Defaults to true.</para>
        /// </summary>
        public bool ShowAddWebAuthn { get; set; } = true;
        [JsonProperty] internal bool AddWebAuthnEnabled => ShowAddWebAuthn 
            && AddWebAuthnLogic != null && CreateWebAuthnRegistrationOptionsLogic != null
            && !string.IsNullOrWhiteSpace(Username);

        /// <summary>
        /// Required for <see cref="AddWebAuthnLogic"/> to function.
        /// <para>Should return the webauthn credentials create options object.</para>
        /// </summary>
        [JsonIgnore]
        public CreateWebAuthnRegistrationOptionsDelegate CreateWebAuthnRegistrationOptionsLogic { get; set; }
        /// <summary>Signature for creating the webauthn credentials create options object. Password should be validated.</summary>
        public delegate HCGenericResult<object> CreateWebAuthnRegistrationOptionsDelegate(string username, string password);

        /// <summary>
        /// If set, allows removing the users WebAuthn binding from the profile page.
        /// <para>Input string is the users password to validate.</para>
        /// </summary>
        [JsonIgnore]
        public RemoveWebAuthnDelegate RemoveWebAuthnLogic { get; set; }
        /// <summary>Signature for removing WebAuthn.</summary>
        public delegate HCGenericResult RemoveWebAuthnDelegate(string password);
        /// <summary>
        /// Allows removing the users WebAuthn binding from the profile page, requires <see cref="RemoveWebAuthnLogic"/> to be set.
        /// <para>Defaults to true.</para>
        /// </summary>
        public bool ShowRemoveWebAuthn { get; set; } = true;
        [JsonProperty] internal bool RemoveWebAuthnEnabled => ShowRemoveWebAuthn && RemoveWebAuthnLogic != null;
    }
}
