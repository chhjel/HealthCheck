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
        /// Adds a new item to the first registered stream of the given type if any.
        /// <para>If <paramref name="handleDuplicates"/> is true and an existing item with the same item id is found, <see cref="IHCDataRepeaterStream.HandleAddedDuplicateItemAsync"/> will be called.</para>
        /// </summary>
        Task AddStreamItemAsync<TStream>(IHCDataRepeaterStreamItem item, object hint = null, bool analyze = true, bool handleDuplicates = true);

        /// <summary>
        /// Attempts to analyze an item.
        /// </summary>
        Task<HCDataRepeaterItemAnalysisResult> AnalyzeItemAsync(string streamId, IHCDataRepeaterStreamItem item);

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
