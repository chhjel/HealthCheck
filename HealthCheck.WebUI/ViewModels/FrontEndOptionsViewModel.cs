using HealthCheck.WebUI.Exceptions;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace HealthCheck.WebUI.ViewModels
{
    /// <summary>
    /// Various front-end options for the web interface.
    /// </summary>
    public class FrontEndOptionsViewModel
    {
        /// <summary>
        /// Title of the page application.
        /// </summary>
        public string ApplicationTitle { get; set; } = "Health Check";

        /// <summary>
        /// Include current query string in API calls to backend.
        /// <para>Enabled by default, can be used to set some roles based on query strings etc.</para>
        /// </summary>
        public bool InludeQueryStringInApiCalls { get; set; } = true;

        /// <summary>
        /// Url to the endpoint that returns tests.
        /// <para>Is set from the constructor relative to the provided baseApiEndpoint.</para>
        /// </summary>
        public string GetTestsEndpoint { get; set; }

        /// <summary>
        /// Url to the endpoint that executes tests.
        /// <para>Is set from the constructor relative to the provided baseApiEndpoint.</para>
        /// </summary>
        public string ExecuteTestEndpoint { get; set; }

        /// <summary>
        /// Url to the endpoint that retrieves events.
        /// <para>Is set from the constructor relative to the provided baseApiEndpoint.</para>
        /// </summary>
        public string GetSiteEventsEndpoint { get; set; }

        /// <summary>
        /// Url to the endpoint that retrieves filtered audit log events.
        /// <para>Is set from the constructor relative to the provided baseApiEndpoint.</para>
        /// </summary>
        public string GetFilteredAuditLogEventsEndpoint { get; set; }

        /// <summary>
        /// Pages that will be shown.
        /// </summary>
        [JsonProperty]
        internal List<string> Pages = new List<string>();

        /// <summary>
        /// Create a new <see cref="FrontEndOptionsViewModel"/>.
        /// </summary>
        /// <param name="baseApiEndpoint"></param>
        public FrontEndOptionsViewModel(string baseApiEndpoint)
        {
            ExecuteTestEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/ExecuteTest";
            GetTestsEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/GetTests";
            GetSiteEventsEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/GetSiteEvents";
            GetFilteredAuditLogEventsEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/GetFilteredAudits";
        }

        /// <summary>
        /// Validates values and throws <see cref="ConfigValidationException"/> if things are missing.
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(GetTestsEndpoint))
                throw new ConfigValidationException($"{nameof(GetTestsEndpoint)} is not set.");
            if (string.IsNullOrWhiteSpace(ExecuteTestEndpoint))
                throw new ConfigValidationException($"{nameof(ExecuteTestEndpoint)} is not set.");
        }
    }
}
