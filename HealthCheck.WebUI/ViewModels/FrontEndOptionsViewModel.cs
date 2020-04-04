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
        /// Text on the Tests-tab. Defaults to "Tests".
        /// </summary>
        public string TestsTabName { get; set; } = "Tests";

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
        /// Number of minutes past the end of a site event it will be displayed below "Current status" on the status page.
        /// <para>Defaults to 30 minutes.</para>
        /// </summary>
        public int CurrentEventBufferMinutes { get; set; } = 30;

        /// <summary>
        /// Default value for custom columns regex pattern/delimiter.
        /// <para>If regex, group names become headers, and groups without names are hidden. Timestamps are always shown in the first column.</para>
        /// </summary>
        public string DefaultColumnRule { get; set; } = "(.*,[0-9]{3}) \\[(?<Thread>[0-9]+)\\] (?<Severity>\\w+) (?<Message>[^\\n]*)";

        /// <summary>
        /// Default column mode will be set to Regex.
        /// </summary>
        public bool DefaultColumnModeIsRegex { get; set; } = true;

        /// <summary>
        /// Enables custom column rule by default.
        /// </summary>
        public bool ApplyCustomColumnRuleByDefault { get; set; }

        /// <summary>
        /// Max number of entries for the insights chart.
        /// <para>Defaults to 5000.</para>
        /// </summary>
        public int MaxInsightsEntryCount { get; set; } = 5000;

        /// <summary>
        /// Show a diagram sandbox.
        /// <para>Defaults to false.</para>
        /// </summary>
        public bool EnableDiagramSandbox { get; set; }

        /// <summary>
        /// Show extra diagram details.
        /// <para>For sequence diagrams class/method names will be shown on step clicks.</para>
        /// <para>Defaults to false.</para>
        /// </summary>
        public bool EnableDiagramDetails { get; set; }

        /// <summary>
        /// Number log searches currently running.
        /// </summary>
        [JsonProperty]
        internal int CurrentlyRunningLogSearchCount { get; set; }

        /// <summary>
        /// Has access to clearing the requestlog.
        /// </summary>
        [JsonProperty]
        internal bool HasAccessToClearRequestLog { get; set; }

        /// <summary>
        /// Default page to show on entering the Index action in prioritized order.
        /// <para>The first available page will be shown when ?page=x is omitted.</para>
        /// <para>Defaults to overview, tests, request log, logviewer, auditlog.</para>
        /// <para>Types omitted will be placed last.</para>
        /// </summary>
        [JsonIgnore]
        public List<HealthCheckPageType> PagePriority { get; set; } = new List<HealthCheckPageType>()
        {
            HealthCheckPageType.Overview,
            HealthCheckPageType.Tests,
            HealthCheckPageType.RequestLog,
            HealthCheckPageType.EventNotifications,
            HealthCheckPageType.Dataflow,
            HealthCheckPageType.Documentation,
            HealthCheckPageType.LogViewer,
            HealthCheckPageType.AuditLog
        };

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

            HasAccessToClearRequestLog = true;
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
