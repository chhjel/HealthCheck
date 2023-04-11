using EPiServer.Framework.Blobs;
using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Modules.Jobs.Abstractions;
using QoDL.Toolkit.Core.Modules.Jobs.Models;
using QoDL.Toolkit.Core.Util.Storage;
using QoDL.Toolkit.Episerver.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Episerver.Storage;

/// <summary>
/// Stores job history details.
/// </summary>
public class TKEpiserverBlobDataJobsHistoryDetailsStorage
    : TKSingleBufferedListBlobStorageBase<TKEpiserverBlobDataJobsHistoryDetailsStorage.TKJobHistoryDetailEntryBlobData, TKJobHistoryDetailEntry>, ITKJobsHistoryDetailsStorage
{
    /// <summary>
    /// Container id used if not overridden.
    /// </summary>
    protected virtual Guid DefaultContainerId => Guid.Parse("888bdf8e-5d5b-4c99-985a-d000191321ec");

    /// <summary>
    /// Defaults to the default provider if null.
    /// </summary>
    public string ProviderName { get; set; }

    /// <summary>
    /// Defaults to a hardcoded guid if null
    /// </summary>
    public Guid? ContainerId { get; set; }

    /// <summary>
    /// Shortcut to <c>ContainerId ?? DefaultContainerId</c>
    /// </summary>
    protected Guid ContainerIdWithFallback => ContainerId ?? DefaultContainerId;

    /// <inheritdoc />
    protected override string CacheKey => $"__tk_{ContainerIdWithFallback}";

    private readonly TKEpiserverBlobHelper<TKJobHistoryDetailEntryBlobData> _blobHelper;

    /// <summary>
    /// Stores job history details.
    /// </summary>
    public TKEpiserverBlobDataJobsHistoryDetailsStorage(IBlobFactory blobFactory, ITKCache cache)
        : base(cache)
    {
        _blobHelper = new TKEpiserverBlobHelper<TKJobHistoryDetailEntryBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
        MaxItemCount = 100;
    }

    /// <inheritdoc />
    public Task<TKJobHistoryDetailEntry> InsertDetailAsync(TKJobHistoryDetailEntry detail)
    {
        InsertItemBuffered(detail);
        return Task.FromResult(detail);
    }

    /// <inheritdoc />
    public Task<TKJobHistoryDetailEntry> GetDetailAsync(Guid id)
        => Task.FromResult(GetItems().FirstOrDefault(x => x.Id == id));

    /// <inheritdoc />
    public Task DeleteDetailAsync(Guid id)
    {
        RemoveMatching(x => x.Id == id);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteAllDetailsAsync()
    {
        RemoveMatching(x => true);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteAllDetailsForJobAsync(string sourceId, string jobId)
    {
        RemoveMatching(x => x.SourceId == sourceId && x.JobId == jobId);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override TKJobHistoryDetailEntryBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

    /// <inheritdoc />
    protected override void StoreBlobData(TKJobHistoryDetailEntryBlobData data) => _blobHelper.StoreBlobData(data);

    /// <summary>
    /// Model stored in blob storage.
    /// </summary>
    public class TKJobHistoryDetailEntryBlobData : IBufferedBlobListStorageData
    {
        /// <summary>
        /// All stored data.
        /// </summary>
        public List<TKJobHistoryDetailEntry> Items { get; set; } = new List<TKJobHistoryDetailEntry>();
    }
}
