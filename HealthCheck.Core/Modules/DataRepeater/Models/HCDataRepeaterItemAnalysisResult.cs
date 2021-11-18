using System.Collections.Generic;

namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary></summary>
    public class HCDataRepeaterItemAnalysisResult
    {
        /// <summary>
        /// Message that is returned to the UI when analysis is executed manually on an item.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Optionally set to true to prevent storing the item when analysis is executed before storage.
        /// <para>Allows for optionally calling store on all items and exclude some during analysis.</para>
        /// </summary>
        public bool DontStore { get; set; }

        /// <summary>
        /// Optionally set if item can be retried or not.
        /// </summary>
        public bool? AllowRetry { get; set; }

        /// <summary>
        /// Tags that will be applied if missing.
        /// </summary>
        public List<string> TagsThatShouldExist { get; set; } = new();

        /// <summary>
        /// Tags that will be removed if present.
        /// </summary>
        public List<string> TagsThatShouldNotExist { get; set; } = new();
    }
}
