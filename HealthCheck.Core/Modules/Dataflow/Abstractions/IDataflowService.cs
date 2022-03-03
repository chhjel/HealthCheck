using HealthCheck.Core.Modules.Dataflow.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Dataflow.Abstractions
{
    /// <summary>
    /// Handles multiple streams for use in the dataflow page.
    /// </summary>
    public interface IDataflowService<TAccessRole>
    {
        /// <summary>
        /// Get metadata for all the available streams.
        /// </summary>
        List<DataflowStreamMetadata<TAccessRole>> GetStreamMetadata();

        /// <summary>
        /// Get metadata for all the available searches.
        /// </summary>
        List<DataflowUnifiedSearchMetadata<TAccessRole>> GetUnifiedSearchesMetadata();

        /// <summary>
        /// Get filtered entries from the given stream.
        /// </summary>
        Task<IEnumerable<IDataflowEntry>> GetEntries(string streamId, DataflowStreamFilter filter);

        /// <summary>
        /// Simple search across streams.
        /// </summary>
        Task<HCDataflowUnifiedSearchResult> UnifiedSearchAsync(string searchId, string query, int pageIndex, int pageSize);
    }
}
