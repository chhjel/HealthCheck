using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Dataflow.Models
{
    /// <summary>
    /// Result object from unified search stream.
    /// </summary>
    public class HCDataflowUnifiedSearchStreamResult
    {
        /// <summary>
        /// Stream the results are from.
        /// </summary>
        public string StreamId { get; set; }

        /// <summary>
        /// Name of the stream the results are from.
        /// </summary>
        public string StreamName { get; set; }

        /// <summary>
        /// Results.
        /// </summary>
        public List<HCDataflowUnifiedSearchResultItem> Entries { get; set; } = new();
    }
}
