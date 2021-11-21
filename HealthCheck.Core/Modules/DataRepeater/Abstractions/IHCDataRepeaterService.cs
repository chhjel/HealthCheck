using HealthCheck.Core.Modules.DataRepeater.Models;
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
        IEnumerable<IHCDataRepeaterStream> GetStreams();

        /// <summary>
        /// Attempts to retry an item.
        /// </summary>
        Task<HCDataRepeaterRetryResult> RetryItemAsync(string streamId, IHCDataRepeaterStreamItem item);

        /// <summary>
        /// Performs an action on an item.
        /// </summary>
        Task<HCDataRepeaterStreamItemActionResult> PerformItemAction(string streamId, string actionId, IHCDataRepeaterStreamItem item, Dictionary<string, string> parameters);
    }
}
