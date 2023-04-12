using EPiServer.Framework.Blobs;
using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Modules.Messages.Abstractions;
using QoDL.Toolkit.Core.Modules.Messages.Models;
using QoDL.Toolkit.Core.Util.Storage;
using QoDL.Toolkit.Episerver.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Episerver.Storage;

/// <summary>
/// Stores messages in blob storage.
/// </summary>
public class TKEpiserverBlobMessagesStore<TMessageModel>
    : TKSingleBufferedMultiListBlobStorageBase<TKEpiserverBlobMessagesStore<TMessageModel>.TKMessagesBlobData, TMessageModel, string>, ITKMessageStorage
    where TMessageModel : class, ITKMessageItem
{
    /// <summary>
    /// Container id used if not overridden.
    /// </summary>
    protected virtual Guid DefaultContainerId => Guid.Parse("882175a0-28b2-4f5f-9acd-5c135666f08e");

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

    private readonly TKEpiserverBlobHelper<TKMessagesBlobData> _blobHelper;

    /// <summary>
    /// Stores messages in blob storage.
    /// </summary>
    public TKEpiserverBlobMessagesStore(IBlobFactory blobFactory, ITKCache cache)
        : base(cache)
    {
        MaxBufferSize = 2500;
        _blobHelper = new TKEpiserverBlobHelper<TKMessagesBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
    }

    /// <inheritdoc />
    public TKDataWithTotalCount<IEnumerable<ITKMessageItem>> GetLatestMessages(string inboxId, int pageSize, int pageIndex)
    {
        var allItems = GetItems(inboxId).ToArray();
        var totalCount = allItems.Length;
        var items = allItems
            .OfType<ITKMessageItem>()
            .Skip(pageIndex * pageSize)
            .Take(pageSize);

        return new TKDataWithTotalCount<IEnumerable<ITKMessageItem>>
        {
            TotalCount = totalCount,
            Data = items ?? Enumerable.Empty<ITKMessageItem>()
        };
    }

    /// <inheritdoc />
    public ITKMessageItem GetMessage(string inboxId, string messageId)
    {
        var items = GetItems(inboxId);
        return items.FirstOrDefault(x => x.Id == messageId);
    }

    /// <inheritdoc />
    public void StoreMessage(string inboxId, ITKMessageItem message)
        => InsertItemBuffered(message as TMessageModel, id: message.Id, groupId: inboxId);

    /// <inheritdoc />
    public void DeleteAllData()
    {
        var data = GetBlobData();
        data.Lists.Clear();
        SaveBlobData(data);
    }

    /// <inheritdoc />
    public bool DeleteInbox(string inboxId)
    {
        var data = GetBlobData();
        if (data.Lists.ContainsKey(inboxId))
        {
            data.Lists.Remove(inboxId);
            SaveBlobData(data);
        }
        return true;
    }

    /// <inheritdoc />
    public bool DeleteMessage(string inboxId, string messageId)
    {
        var data = GetBlobData();
        if (data.Lists.ContainsKey(inboxId))
        {
            data.Lists[inboxId].RemoveAll(x => x.Id == messageId);
            SaveBlobData(data);
        }
        return true;
    }

    /// <inheritdoc />
    protected override TKMessagesBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

    /// <inheritdoc />
    protected override void StoreBlobData(TKMessagesBlobData data) => _blobHelper.StoreBlobData(data);

    /// <summary>
    /// Model stored in blob storage.
    /// </summary>
    public class TKMessagesBlobData : IBufferedBlobMultiListStorageData
    {
        /// <inheritdoc />
        public Dictionary<string, List<TMessageModel>> Lists { get; set; } = new Dictionary<string, List<TMessageModel>>();
    }
}
