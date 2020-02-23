using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Dataflow
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
        /// Get filtered entries from the given stream.
        /// </summary>
        Task<IEnumerable<IDataflowEntry>> GetEntries(string streamId, DataflowStreamFilter filter);
    }

}
