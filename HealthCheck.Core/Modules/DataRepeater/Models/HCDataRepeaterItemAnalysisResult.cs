using System.Collections.Generic;

namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary></summary>
    public class HCDataRepeaterItemAnalysisResult
    {
        /// <summary>
        /// Message that is returned to the UI when analysis is executed manually on an item.
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// Optionally set if item can be retried or not.
        /// </summary>
        public bool? AllowRetry { get; set; }

        /// <summary>
        /// Tags that will be applied if missing.
        /// </summary>
        public IEnumerable<string> TagsThatShouldBeApplied { get; set; }
    }
}
