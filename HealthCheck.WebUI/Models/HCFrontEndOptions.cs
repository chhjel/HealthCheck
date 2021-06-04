using HealthCheck.WebUI.Exceptions;
using Newtonsoft.Json;
using System;
using static HealthCheck.WebUI.Models.HCIntegratedLoginConfig;

namespace HealthCheck.WebUI.Models
{
    /// <summary>
    /// Various front-end options for the web interface.
    /// </summary>
    public class HCFrontEndOptions
    {
        /// <summary>
        /// Title of the page application.
        /// </summary>
        public string ApplicationTitle { get; set; } = "Health Check";

        /// <summary>
        /// Optional url the application title links to.
        /// </summary>
        public string ApplicationTitleLink { get; set; }

        /// <summary>
        /// Url to the endpoint.
        /// </summary>
        public string EndpointBase { get; set; }

        /// <summary>
        /// Url to the endpoint that invokes module methods.
        /// </summary>
        public string InvokeModuleMethodEndpoint { get; set; }

        /// <summary>
        /// Optional logout url title.
        /// <para>Defaults to "Logout"</para>
        /// </summary>
        public string LogoutLinkTitle { get; set; } = "Logout";

        /// <summary>
        /// Optional logout url.
        /// <para>Defaults to null, hiding it.</para>
        /// </summary>
        public string LogoutLinkUrl { get; set; }

        /// <summary>
        /// Include current query string in API calls to backend.
        /// <para>Enabled by default, can be used to set some roles based on query strings etc.</para>
        /// </summary>
        public bool InludeQueryStringInApiCalls { get; set; } = true;

        /// <summary>
        /// Config for the code editors.
        /// </summary>
        public EditorWorkerConfig EditorConfig { get; set; } = new EditorWorkerConfig();

        [JsonProperty]
        internal bool ShowIntegratedLogin { get; set; }

        [JsonProperty]
        internal string IntegratedLoginEndpoint { get; set; }

        [JsonProperty]
        internal DateTimeOffset? IntegratedLoginCurrent2FACodeExpirationTime { get; set; }

        [JsonProperty]
        internal int IntegratedLogin2FACodeLifetime { get; set; } = 30;

        [JsonProperty]
        internal string IntegratedLoginSend2FACodeEndpoint { get; set; }

        [JsonProperty]
        internal string IntegratedLoginSend2FACodeButtonText { get; set; }

        [JsonProperty]
        internal HCLoginWebAuthnMode IntegratedLoginWebAuthnMode { get; set; }

        [JsonProperty]
        internal HCLoginTwoFactorCodeInputMode IntegratedLoginTwoFactorCodeInputMode { get; set; }

        /// <summary>
        /// Create a new <see cref="HCFrontEndOptions"/>.
        /// </summary>
        /// <param name="baseApiEndpoint"></param>
        public HCFrontEndOptions(string baseApiEndpoint)
        {
            EndpointBase = baseApiEndpoint;
            InvokeModuleMethodEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/InvokeModuleMethod";
        }

        /// <summary>
        /// Validates values and throws <see cref="ConfigValidationException"/> if things are missing.
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(InvokeModuleMethodEndpoint))
                throw new ConfigValidationException($"{nameof(InvokeModuleMethodEndpoint)} is not set.");
        }

        /// <summary>
        /// Config for the monaco-editor.
        /// </summary>
        public class EditorWorkerConfig
        {
            /// <summary>
            /// Url to the editor worker script.
            /// <para>Can be prefixed with "blob:" to proxy it through a generated blob url.</para>
            /// </summary>
            public string EditorWorkerUrl { get; set; } = "blob:https://unpkg.com/christianh-healthcheck@2/editor.worker.js";

            /// <summary>
            /// Url to the json worker script.
            /// <para>Can be prefixed with "blob:" to proxy it through a generated blob url.</para>
            /// </summary>
            public string JsonWorkerUrl { get; set; } = "blob:https://unpkg.com/christianh-healthcheck@2/json.worker.js";
        }
    }
}
