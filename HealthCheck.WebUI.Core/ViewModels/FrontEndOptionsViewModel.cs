using HealthCheck.Web.Core.Exceptions;

namespace HealthCheck.Web.Core.ViewModels
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
        /// </summary>
        public string GetTestsEndpoint { get; set; }

        /// <summary>
        /// Url to the endpoint that executes tests.
        /// </summary>
        public string ExecuteTestEndpoint { get; set; }

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
