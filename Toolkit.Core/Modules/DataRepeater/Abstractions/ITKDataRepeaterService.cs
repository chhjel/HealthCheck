using QoDL.Toolkit.Core.Modules.DataRepeater.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;

/// <summary>
/// Handles repeater streams for reprocessing data.
/// </summary>
public interface ITKDataRepeaterService
{
    /// <summary>
    /// Get all configured streams.
    /// </summary>
    IEnumerable<ITKDataRepeaterStream> GetStreams();

    /// <summary>
    /// Adds a new item to the first registered stream of the given type if any.
    /// <para>If <paramref name="handleDuplicates"/> is true and an existing item with the same item id is found, <see cref="ITKDataRepeaterStream.HandleAddedDuplicateItemAsync"/> will be called.</para>
    /// </summary>
    Task AddStreamItemAsync<TStream>(ITKDataRepeaterStreamItem item, object hint = null, bool analyze = true, bool handleDuplicates = true);

    /// <summary>
    /// Adds a batch of items to the first registered stream of the given type if any.
    /// <para>This method is much more optimized than calling <see cref="AddStreamItemAsync"/> many times.</para>
    /// <para>If <paramref name="handleDuplicates"/> is true and an existing item with the same item id is found, <see cref="ITKDataRepeaterStream.HandleAddedDuplicateItemAsync"/> will be called.</para>
    /// </summary>
    Task AddStreamItemsAsync<TStream>(IEnumerable<ITKDataRepeaterStreamItem> items, object hint = null, bool analyze = true, bool handleDuplicates = true);

    /// <summary>
    /// Attempts to analyze an item.
    /// </summary>
    Task<TKDataRepeaterItemAnalysisResult> AnalyzeItemAsync(string streamId, ITKDataRepeaterStreamItem item);

    /// <summary>
    /// Attempts to retry an item.
    /// </summary>
    Task<TKDataRepeaterRetryResult> RetryItemAsync(string streamId, ITKDataRepeaterStreamItem item);

    /// <summary>
    /// Performs an action on an item.
    /// </summary>
    Task<TKDataRepeaterStreamItemActionResult> PerformItemAction(string streamId, string actionId, ITKDataRepeaterStreamItem item, Dictionary<string, string> parameters);

    /// <summary>
    /// Performs a batch action on all the items in this stream.
    /// </summary>
    Task<TKDataRepeaterStreamBatchActionResult> PerformItemBatchAction(string streamId, string actionId, Dictionary<string, string> parameters);
}
