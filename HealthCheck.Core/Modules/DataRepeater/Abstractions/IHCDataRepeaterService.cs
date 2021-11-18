using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.DataRepeater.Abstractions
{
    /// <summary>
    /// Handles repeater streams for reprocessing data.
    /// </summary>
    public interface IHCDataRepeaterService
    {
        /// <summary>
        /// Get all configured streams.
        /// </summary>
        Task<IEnumerable<IHCDataRepeaterStream>> GetStreamsAsync();
    }
}
