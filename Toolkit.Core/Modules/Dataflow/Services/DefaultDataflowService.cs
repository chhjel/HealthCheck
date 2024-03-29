using QoDL.Toolkit.Core.Modules.Dataflow.Abstractions;
using QoDL.Toolkit.Core.Modules.Dataflow.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.Dataflow.Services;

/// <summary>
/// Default implementation of <see cref="IDataflowService{TAccessRole}"/>.
/// <para>Allows for multiple streams to be registered in the options object.</para>
/// </summary>
public class DefaultDataflowService<TAccessRole> : IDataflowService<TAccessRole>
{
    private DefaultDataflowServiceOptions<TAccessRole> Options { get; }

    /// <summary>
    /// Default implementation of <see cref="IDataflowService{TAccessRole}"/>.
    /// <para>Allows for multiple streams to be registered in the options object.</para>
    /// </summary>
    public DefaultDataflowService(DefaultDataflowServiceOptions<TAccessRole> options)
    {
        Options = options;
    }

    /// <summary>
    /// Get metadata for all the available streams.
    /// </summary>
    public List<DataflowStreamMetadata<TAccessRole>> GetStreamMetadata()
    {
        return (Options.Streams ?? Enumerable.Empty<IDataflowStream<TAccessRole>>())
            .Where(x => x.IsVisible?.Invoke() != false)
            .Select(x => new DataflowStreamMetadata<TAccessRole>
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                GroupName = x.GroupName,
                SupportsFilterByDate = x.SupportsFilterByDate,
                DateTimePropertyNameForUI = x.DateTimePropertyNameForUI,
                PropertyDisplayInfo = x.GetEntryPropertiesInfo()?.ToList() ?? new List<DataFlowPropertyDisplayInfo>(),
                RolesWithAccess = x.RolesWithAccess
            })
            .ToList();
    }

    /// <summary>
    /// Get metadata for all the available searches.
    /// </summary>
    public List<DataflowUnifiedSearchMetadata<TAccessRole>> GetUnifiedSearchesMetadata()
    {
        return (Options.UnifiedSearches ?? Enumerable.Empty<ITKDataflowUnifiedSearch<TAccessRole>>())
            .Where(x => x.IsVisible?.Invoke() != false)
            .Select(x => new DataflowUnifiedSearchMetadata<TAccessRole>
            {
                Id = x.GetType().FullName,
                Name = x.Name,
                RolesWithAccess = x.RolesWithAccess,
                Description = x.Description,
                QueryPlaceholder = x.QueryPlaceholder,
                GroupName = x.GroupName,
                GroupByLabel = x.GroupByLabel,
                StreamNamesOverrides = (x.StreamNamesOverrides ?? new Dictionary<System.Type, string>()).ToDictionary(x => x.Key.FullName, x => x.Value),
                GroupByStreamNamesOverrides = (x.GroupByStreamNamesOverrides ?? new Dictionary<System.Type, string>()).ToDictionary(x => x.Key.FullName, x => x.Value)
            })
            .ToList();
    }

    /// <summary>
    /// Get filtered entries from the given stream.
    /// </summary>
    public async Task<IEnumerable<IDataflowEntry>> GetEntries(string streamId, DataflowStreamFilter filter)
    {
        var stream = Options.Streams?.FirstOrDefault(x => x.Id == streamId);
        if (stream == null || stream.IsVisible?.Invoke() == false)
        {
            return Enumerable.Empty<IDataflowEntry>();
        }

        return await stream.GetLatestStreamEntriesAsync(filter);
    }

    /// <inheritdoc />
    public async Task<TKDataflowUnifiedSearchResult> UnifiedSearchAsync(string searchId, string query, int pageIndex, int pageSize)
    {
        var result = new TKDataflowUnifiedSearchResult();

        var search = Options.UnifiedSearches?.FirstOrDefault(x => x.GetType().FullName == searchId);
        var streamsToSearch = search.StreamTypesToSearch
            ?.Select(x => Options.Streams?.FirstOrDefault(s => s.GetType() == x))
            ?.ToArray();
        if (streamsToSearch?.Any() != true) return result;

        foreach (var stream in streamsToSearch)
        {
            var streamType = stream.GetType();
            var propertyFilter = search.CreateStreamPropertyFilter(stream, query);
            var filter = new DataflowStreamFilter
            {
                PropertyFilters = propertyFilter,
                Skip = pageIndex * pageSize,
                Take = pageSize
            };

            var entries = await stream.GetLatestStreamEntriesAsync(filter);
            var resultEntries = entries.Select(e => search.CreateResultItem(streamType, e)).ToList();

            var streamResult = new TKDataflowUnifiedSearchStreamResult
            {
                StreamId = stream.GetType().FullName,
                StreamName = stream.Name,
                Entries = resultEntries,
            };
            result.StreamResults.Add(streamResult);
        }

        return result;
    }
}
