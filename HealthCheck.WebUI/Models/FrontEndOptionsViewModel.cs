using HealthCheck.Core.Enums;
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

        #region Endpoints
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
        /// Url to the endpoint that cancels tests.
        /// <para>Is set from the constructor relative to the provided baseApiEndpoint.</para>
        /// </summary>
        public string CancelTestEndpoint { get; set; }

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
        /// Url to the endpoint that searches logs.
        /// <para>Is set from the constructor relative to the provided baseApiEndpoint.</para>
        /// </summary>
        public string GetLogSearchResultsEndpoint { get; set; }

        /// <summary>
        /// Url to the endpoint that cancels logs searches.
        /// <para>Is set from the constructor relative to the provided baseApiEndpoint.</para>
        /// </summary>
        public string CancelLogSearchEndpoint { get; set; }

        /// <summary>
        /// Url to the endpoint that cancels all logs searches.
        /// <para>Is set from the constructor relative to the provided baseApiEndpoint.</para>
        /// </summary>
        public string CancelAllLogSearchesEndpoint { get; set; }

        /// <summary>
        /// Url to the endpoint that returns all request log entries.
        /// </summary>
        public string GetRequestLogEndpoint { get; set; }

        /// <summary>
        /// Url to the endpoint that clears all request log entries.
        /// </summary>
        public string ClearRequestLogEndpoint { get; set; }

        /// <summary>
        /// Url to the endpoint that returns diagram data.
        /// </summary>
        public string DiagramsDataEndpoint { get; set; }

        /// <summary>
        /// Url to the endpoint that returns dataflow stream metadata.
        /// </summary>
        public string GetDataflowStreamEntriesEndpoint { get; set; }

        /// <summary>
        /// Url to the endpoint that returns dataflow stream entries.
        /// </summary>
        public string GetDataflowStreamsMetadataEndpoint { get; set; }

        /// <summary>
        /// Url to the endpoint where settings are retrieved from.
        /// </summary>
        public string GetSettingsEndpoint { get; set; }

        /// <summary>
        /// Url to the endpoint where settings are updated.
        /// </summary>
        public string SetSettingsEndpoint { get; set; }

        /// <summary>
        /// Url to the endpoint where event notification configs are retrieved from.
        /// </summary>
        public string GetEventNotificationConfigsEndpoint { get; set; }

        /// <summary>
        /// Url to the endpoint where event notification configs are saved.
        /// </summary>
        public string SaveEventNotificationConfigEndpoint { get; set; }

        /// <summary>
        /// Url to the endpoint where event notification configs are deleted.
        /// </summary>
        public string DeleteEventNotificationConfigEndpoint { get; set; }

        /// <summary>
        /// Url to the endpoint where event notification configs are toggled.
        /// </summary>
        public string SetEventNotificationConfigEnabledEndpoint { get; set; }

        /// <summary>
        /// Url to the endpoint where event notification definitions are deleted.
        /// </summary>
        public string DeleteEventDefinitionEndpoint  { get; set; }

        /// <summary>
        /// Url to the endpoint where event notification definitions are deleted.
        /// </summary>
        public string DeleteEventDefinitionsEndpoint { get; set; }
        #endregion

        #region EventNotifications
        /// <summary>
        /// Has access to editing event notification definitions.
        /// </summary>
        [JsonProperty]
        internal bool HasAccessToEditEventDefinitions { get; set; }
        #endregion

        /// <summary>
        /// Create a new <see cref="FrontEndOptionsViewModel"/>.
        /// </summary>
        /// <param name="baseApiEndpoint"></param>
        public FrontEndOptionsViewModel(string baseApiEndpoint)
        {
            InvokeModuleMethodEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/InvokeModuleMethod";

            ExecuteTestEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/ExecuteTest";
            CancelTestEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/CancelTest";
            GetTestsEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/GetTests";
            GetSiteEventsEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/GetSiteEvents";
            GetFilteredAuditLogEventsEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/GetFilteredAudits";
            GetLogSearchResultsEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/SearchLogs";
            CancelLogSearchEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/CancelLogSearch";
            CancelAllLogSearchesEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/CancelAllLogSearches";
            GetRequestLogEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/GetRequestLog";
            ClearRequestLogEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/ClearRequestLog";
            DiagramsDataEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/GetDiagrams";
            GetDataflowStreamsMetadataEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/GetDataflowStreamsMetadata";
            GetDataflowStreamEntriesEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/GetDataflowStreamEntries";
            GetSettingsEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/GetSettings";
            SetSettingsEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/SetSettings";
            GetEventNotificationConfigsEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/GetEventNotificationConfigs";
            DeleteEventNotificationConfigEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/DeleteEventNotificationConfig";
            SaveEventNotificationConfigEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/SaveEventNotificationConfig";
            SetEventNotificationConfigEnabledEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/SetEventNotificationConfigEnabled";
            DeleteEventDefinitionEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/DeleteEventDefinition";
            DeleteEventDefinitionsEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/DeleteEventDefinitions";

            HasAccessToEditEventDefinitions = true;
        }

        /// <summary>
        /// Validates values and throws <see cref="ConfigValidationException"/> if things are missing.
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(InvokeModuleMethodEndpoint))
                throw new ConfigValidationException($"{nameof(InvokeModuleMethodEndpoint)} is not set.");
        }
    }
}
