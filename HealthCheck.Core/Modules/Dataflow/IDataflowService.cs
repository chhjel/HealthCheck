using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Dataflow
{
    /// <summary>
    /// Handles multiple streams for use in the dataflow page.
    /// </summary>
    public interface IDataflowService
    {
        /// <summary>
        /// Get metadata for all the available streams.
        /// </summary>
        List<DataflowStreamMetadata> GetStreamMetadata();

        /// <summary>
        /// Get filtered entries from the given stream.
        /// </summary>
        Task<IEnumerable<IDataflowEntry>> GetEntries(string streamId, DataflowStreamFilter filter);
    }

}
