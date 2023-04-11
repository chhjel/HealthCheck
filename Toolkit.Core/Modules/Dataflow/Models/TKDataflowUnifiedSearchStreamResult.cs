using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Dataflow.Models
{
    /// <summary>
    /// Result object from unified search stream.
    /// </summary>
    public class TKDataflowUnifiedSearchStreamResult
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
        public List<TKDataflowUnifiedSearchResultItem> Entries { get; set; } = new();
    }
}
