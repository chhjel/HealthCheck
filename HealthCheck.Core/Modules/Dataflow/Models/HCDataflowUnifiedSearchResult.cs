using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Dataflow.Models
{
    /// <summary>
    /// Result object from unified search.
    /// </summary>
    public class HCDataflowUnifiedSearchResult
    {
        /// <summary>
        /// Results per stream.
        /// </summary>
        public List<HCDataflowUnifiedSearchStreamResult> StreamResults { get; set; } = new();
    }
}
