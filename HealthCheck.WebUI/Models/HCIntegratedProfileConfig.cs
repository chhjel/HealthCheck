using HealthCheck.Core.Models;
using Newtonsoft.Json;
using System;

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
        /// If set, allows entering TOTP code from the profile to elevate access.
        /// <para>Input string is the one-time code.</para>
        /// </summary>
        [JsonIgnore]
        public Func<string, HCGenericResult> TotpElevationLogic { get; set; }
        [JsonProperty] internal bool TotpElevationEnabled => TotpElevationLogic != null;

        /// <summary>
        /// If set, allows registering the users TOTP binding from the profile page.
        /// <para>Input string 1 is the TOTP secret to store on the user, input string 2 is a code generated using the secret to be validated.</para>
        /// </summary>
        [JsonIgnore]
        public Func<string, string, HCGenericResult> AddTotpLogic { get; set; }
        [JsonProperty] internal bool AddTotpEnabled => AddTotpLogic != null;

        /// <summary>
        /// If set, allows removing the users TOTP binding from the profile page.
        /// </summary>
        [JsonIgnore]
        public Func<HCGenericResult> RemoveTotpLogic { get; set; }
        [JsonProperty] internal bool RemoveTotpEnabled => RemoveTotpLogic != null;

        /// <summary>
        /// If set, allows authenticating using WebAuthn from the profile to elevate access.
        /// <para>Requires <see cref="Username"/> to be set.</para>
        /// <para>Input string is the webauthn payload.</para>
        /// </summary>
        [JsonIgnore]
        public Func<HCVerifyWebAuthnAssertionModel, HCGenericResult> WebAuthnElevationLogic { get; set; }
        [JsonProperty] internal bool WebAuthnElevationEnabled => WebAuthnElevationLogic != null && !string.IsNullOrWhiteSpace(Username);

        /// <summary>
        /// If set, allows registering the users WebAuthn binding from the profile page.
        /// <para>Requires <see cref="Username"/> to be set.</para>
        /// <para>Input string is the WebAuthn payload in json format to store on the user.</para>
        /// </summary>
        [JsonIgnore]
        public Func<string, HCGenericResult> AddWebAuthnLogic { get; set; }
        [JsonProperty] internal bool AddWebAuthnEnabled => AddWebAuthnLogic != null && !string.IsNullOrWhiteSpace(Username);

        /// <summary>
        /// If set, allows removing the users WebAuthn binding from the profile page.
        /// </summary>
        [JsonIgnore]
        public Func<HCGenericResult> RemoveWebAuthnLogic { get; set; }
        [JsonProperty] internal bool RemoveWebAuthnEnabled => RemoveWebAuthnLogic != null;
    }
}
