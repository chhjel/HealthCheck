using EPiServer.Framework.Blobs;
using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Episerver.Utils;
using QoDL.Toolkit.Core.Util.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Episerver.Storage;

/// <summary>
/// Stores messages in blob storage.
/// </summary>
public class TKEpiserverBufferedBlobDataStoreWithEntryId<TItem, TId>
    : TKSingleBufferedDictionaryBlobStorageBase<TKEpiserverBufferedBlobDataStoreWithEntryId<TItem, TId>.TKBlobDataStoreWithEntryIdData, TItem, TId>, IDataStoreWithEntryId<TItem>
{
    /// <summary>
    /// Container id used if not overridden.
    /// </summary>
    protected virtual Guid DefaultContainerId => Guid.Parse("874e5461-314a-4718-8d09-a441597c50f4");

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

    private readonly TKEpiserverBlobHelper<TKBlobDataStoreWithEntryIdData> _blobHelper;

    private Func<TItem, TId> IdSelector { get; set; }

    /// <summary>
    /// Stores messages in blob storage.
    /// </summary>
    public TKEpiserverBufferedBlobDataStoreWithEntryId(IBlobFactory blobFactory, ITKCache cache, Func<TItem, TId> idSelector)
        : base(cache)
    {
        IdSelector = idSelector;
        _blobHelper = new TKEpiserverBlobHelper<TKBlobDataStoreWithEntryIdData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
    }

    /// <summary>
    /// Store the given item. Replaces item if it already exists.
    /// The <paramref name="update"/> parameter is not used.
    /// </summary>
    public TItem InsertOrUpdateItem(TItem item, Func<TItem, TItem> update = null)
    {
        InsertItemBuffered(item, IdSelector(item));
        return item;
    }

    /// <inheritdoc />
    public void InsertOrUpdateItems(IEnumerable<TItem> items)
        => InsertItemsBuffered(items, IdSelector);

    /// <inheritdoc />
    public IEnumerable<TItem> GetEnumerable()
    {
        foreach (var item in GetBufferedItems())
        {
            yield return item;
        }

        var data = GetBlobData();
        if (data?.Items != null)
        {
            foreach (var item in data.Items.Values ?? Enumerable.Empty<TItem>())
            {
                yield return item;
            }
        }
    }

    /// <inheritdoc />
    protected override TKBlobDataStoreWithEntryIdData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

    /// <inheritdoc />
    protected override void StoreBlobData(TKBlobDataStoreWithEntryIdData data) => _blobHelper.StoreBlobData(data);

    /// <summary>
    /// Model stored in blob storage.
    /// </summary>
    public class TKBlobDataStoreWithEntryIdData : IBufferedBlobDictionaryStorageData
    {
        /// <inheritdoc />
        public Dictionary<TId, TItem> Items { get; set; } = new Dictionary<TId, TItem>();
    }
}
