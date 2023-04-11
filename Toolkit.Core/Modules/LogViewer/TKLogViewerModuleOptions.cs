using QoDL.Toolkit.Core.Abstractions;

namespace QoDL.Toolkit.Core.Modules.LogViewer
{
    /// <summary>
    /// Options for <see cref="TKLogViewerModule"/>.
    /// </summary>
    public class TKLogViewerModuleOptions
    {
        /// <summary>
        /// Must be set to be able to search.
        /// </summary>
        public ILogSearcherService LogSearcherService { get; set; }

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
    }
}
