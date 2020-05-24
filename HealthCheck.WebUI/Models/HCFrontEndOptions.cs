﻿using HealthCheck.WebUI.Exceptions;

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
        /// Url to the endpoint that invokes module methods.
        /// </summary>
        public string InvokeModuleMethodEndpoint { get; set; }

        /// <summary>
        /// Include current query string in API calls to backend.
        /// <para>Enabled by default, can be used to set some roles based on query strings etc.</para>
        /// </summary>
        public bool InludeQueryStringInApiCalls { get; set; } = true;

        /// <summary>
        /// Config for the code editors.
        /// </summary>
        public EditorWorkerConfig EditorConfig { get; set; } = new EditorWorkerConfig();

        /// <summary>
        /// Create a new <see cref="HCFrontEndOptions"/>.
        /// </summary>
        /// <param name="baseApiEndpoint"></param>
        public HCFrontEndOptions(string baseApiEndpoint)
        {
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
            /// </summary>
            public string EditorWorkerUrl { get; set; } = "https://unpkg.com/christianh-healthcheck@2/editor.worker.js";

            /// <summary>
            /// Url to the json worker script.
            /// </summary>
            public string JsonWorkerUrl { get; set; } = "https://unpkg.com/christianh-healthcheck@2/json.worker.js";
        }
    }
}