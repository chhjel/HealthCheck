using EPiServer.Framework.Blobs;
using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Extensions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Models;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Core.Util.Storage;
using QoDL.Toolkit.Episerver.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Episerver.Storage;

/// <summary>
/// Stores datarepeater items.
/// </summary>
/// <example>
/// <![CDATA[
///     public class MyData {}
///     public class MyStreamItem : TKDefaultDataRepeaterStreamItem<MyData, MyStreamItem> {}
///     public class MyStorage : TKEpiserverBlobDataRepeaterStreamItemStorage<MyStreamItem> {}
/// ]]>
/// </example>
public abstract class TKEpiserverBlobDataRepeaterStreamItemStorage<TItem>
    : TKSingleBufferedDictionaryBlobStorageBase<TKEpiserverBlobDataRepeaterStreamItemStorage<TItem>.TKDataRepeaterBlobData, TItem, Guid>, ITKDataRepeaterStreamItemStorage
    where TItem : ITKDataRepeaterStreamItem
{
    /// <summary>
    /// If disabled the service will ignore any attempts to add new data.
    /// <para>Enabled by default. Null value/exception = false.</para>
    /// </summary>
    public Func<bool> AllowInsertNew { get; set; } = () => true;

    /// <summary>
    /// If disabled the service will ignore any attempts to update data.
    /// <para>Enabled by default. Null value/exception = false.</para>
    /// </summary>
    public Func<bool> AllowUpdateExisting { get; set; } = () => true;

    /// <summary>
    /// Defaults to the default provider if null.
    /// </summary>
    public string ProviderName { get; set; }

    /// <summary>
    /// Blob container id.
    /// </summary>
    protected abstract Guid ContainerId { get; }

    /// <inheritdoc />
    protected override string CacheKey => $"__tk_{ContainerId}";

    private readonly TKEpiserverBlobHelper<TKDataRepeaterBlobData> _blobHelper;

    /// <summary>
    /// Stores datarepeater items.
    /// </summary>
    public TKEpiserverBlobDataRepeaterStreamItemStorage(IBlobFactory blobFactory, ITKCache cache)
        : base(cache)
    {
        _blobHelper = new TKEpiserverBlobHelper<TKDataRepeaterBlobData>(blobFactory, () => ContainerId, () => ProviderName);
        BlobUpdateBufferDuration = TimeSpan.FromSeconds(30);
        SupportsMaxItemAge = true;
        SupportsExpirationTime = true;
    }

    #region ITKDataRepeaterStreamItemStorage Implementation
    /// <inheritdoc />
    public Task AddItemAsync(ITKDataRepeaterStreamItem item, object hint = null)
    {
        if (!AllowInsertNewInternal()) return Task.CompletedTask;
        InsertItemBuffered((TItem)item, item.Id);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UpdateItemAsync(ITKDataRepeaterStreamItem item)
    {
        if (!AllowUpdateExisting()) return Task.CompletedTask;
        item.LastUpdatedAt = DateTimeOffset.Now;
        InsertItemBuffered((TItem)item, item.Id, isUpdate: true);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<ITKDataRepeaterStreamItem> GetItemAsync(Guid id)
    {
        var item = GetItems().FirstOrDefault(x => x.Id == id);
        return Task.FromResult(item as ITKDataRepeaterStreamItem);
    }

    /// <inheritdoc />
    public Task<ITKDataRepeaterStreamItem> GetItemByItemIdAsync(string itemId)
    {
        var item = GetItems().FirstOrDefault(x => x.ItemId == itemId);
        return Task.FromResult(item as ITKDataRepeaterStreamItem);
    }

    /// <inheritdoc />
    public Task DeleteItemAsync(Guid id)
    {
        RemoveItem(id);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task AddItemTagAsync(Guid id, string tag)
    {
        var item = await GetItemAsync(id).ConfigureAwait(false);
        if (item != null && item.Tags?.Contains(tag) != true)
        {
            item.Tags ??= new HashSet<string>();
            item.Tags.Add(tag);
            await UpdateItemAsync(item).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task AddItemTagsAsync(Guid id, params string[] tags)
    {
        var item = await GetItemAsync(id).ConfigureAwait(false);
        if (item != null && tags?.Any() == true && tags.Any(t => item.Tags?.Contains(t) != true))
        {
            item.Tags ??= new HashSet<string>();
            foreach (var tag in tags)
            {
                item.Tags.Add(tag);
            }
            await UpdateItemAsync(item).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task RemoveItemTagAsync(Guid id, string tag)
    {
        var item = await GetItemAsync(id).ConfigureAwait(false);
        if (item?.Tags?.Contains(tag) == true)
        {
            item.Tags.Remove(tag);
            await UpdateItemAsync(item).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task RemoveItemTagsAsync(Guid id, params string[] tags)
    {
        var item = await GetItemAsync(id).ConfigureAwait(false);
        if (item != null && tags?.Any() == true && tags.Any(t => item.Tags?.Contains(t) == true))
        {
            item.Tags ??= new HashSet<string>();
            foreach (var tag in tags)
            {
                item.Tags.Remove(tag);
            }
            await UpdateItemAsync(item).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task RemoveAllItemTagsAsync(Guid id)
    {
        var item = await GetItemAsync(id).ConfigureAwait(false);
        if (item != null)
        {
            item.Tags = new HashSet<string>();
            await UpdateItemAsync(item).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task SetAllowItemRetryAsync(Guid id, bool allow)
    {
        var item = await GetItemAsync(id).ConfigureAwait(false);
        if (item != null && item.AllowRetry != allow)
        {
            item.AllowRetry = allow;
            await UpdateItemAsync(item).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task SetItemExpirationTimeAsync(Guid id, DateTimeOffset? time)
    {
        var item = await GetItemAsync(id).ConfigureAwait(false);
        if (item != null && item.ExpirationTime != time)
        {
            item.ExpirationTime = time;
            await UpdateItemAsync(item).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task SetForcedItemStatusAsync(Guid id, TKDataRepeaterStreamItemStatus? status, Maybe<DateTimeOffset?> expirationTime = null, string logMessage = null, string error = null)
    {
        var item = await GetItemAsync(id).ConfigureAwait(false);
        if (item != null)
        {
            item.ForcedStatus = status;
            if (expirationTime != null)
            {
                item.ExpirationTime = expirationTime.Value;
            }
            if (!string.IsNullOrWhiteSpace(logMessage))
            {
                item.AddLogMessage(logMessage);
            }
            if (!string.IsNullOrWhiteSpace(error))
            {
                item.Error = error;
                item.LastErrorAt = DateTimeOffset.Now;
                if (string.IsNullOrWhiteSpace(item.FirstError))
                {
                    item.FirstError = error;
                    item.FirstErrorAt = DateTimeOffset.Now;
                }
            }
            await UpdateItemAsync(item).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task AddItemLogMessageAsync(Guid id, string logMessage)
    {
        var item = await GetItemAsync(id).ConfigureAwait(false);
        if (item == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(logMessage))
        {
            item.AddLogMessage(logMessage);
            await UpdateItemAsync(item).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task SetItemErrorAsync(Guid id, string error)
    {
        var item = await GetItemAsync(id).ConfigureAwait(false);
        if (item == null)
        {
            return;
        }

        var save = false;
        if (error != item.Error)
        {
            save = true;
            item.Error = error;
            item.LastErrorAt = DateTimeOffset.Now;
        }
        if (!string.IsNullOrWhiteSpace(error) && string.IsNullOrWhiteSpace(item.FirstError))
        {
            save = true;
            item.FirstError = error;
            item.FirstErrorAt = DateTimeOffset.Now;
        }

        if (save)
        {
            await UpdateItemAsync(item).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public Task<TKDataRepeaterStreamItemsPagedModel> GetItemsPagedAsync(TKGetDataRepeaterStreamItemsFilteredRequest model)
    {
        var matches = GetItems()
            .Where(x =>
                (string.IsNullOrWhiteSpace(model.Filter)
                || x.ItemId?.ToLower()?.Contains(model.Filter?.ToLower()) == true
                || x.Summary?.ToLower()?.Contains(model.Filter?.ToLower()) == true)
                && (model.Tags?.Any() != true || model.Tags.All(t => x.Tags?.Any(tt => tt?.ToLower() == t.ToLower()) == true) == true)
                && (model.RetryAllowed == null || x.AllowRetry == model.RetryAllowed))
            .OrderByDescending(x => x.InsertedAt)
            .OfType<ITKDataRepeaterStreamItem>()
            .ToArray();
        var items = matches
            .Skip(model.PageIndex * model.PageSize)
            .Take(model.PageSize);
        var result = new TKDataRepeaterStreamItemsPagedModel
        {
            TotalCount = matches.Length,
            Items = items
        };
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<IEnumerable<ITKDataRepeaterStreamItem>> GetAllItemsAsync()
        => Task.FromResult(GetItems().Cast<ITKDataRepeaterStreamItem>());

    /// <inheritdoc />
    public async Task PerformBatchUpdateAsync(TKDataRepeaterBatchedStorageItemActions actions)
    {
        if (actions.Adds?.Any() == true)
        {
            foreach (var action in actions.Adds)
            {
                await AddItemAsync(action.Item, action.Hint);
            }
        }


        if (actions.Updates?.Any() == true)
        {
            foreach (var action in actions.Updates)
            {
                await UpdateItemAsync(action.Item);
            }
        }

        if (actions.Deletes?.Any() == true)
        {
            var deleteIds = new HashSet<string>(actions.Deletes.Select(x => x.Item.ItemId));
            RemoveAllItems(x => deleteIds.Contains(x.ItemId));
        }
    }
    #endregion

    /// <inheritdoc />
    protected override DateTimeOffset GetItemTimestamp(TItem item) => item.InsertedAt;

    /// <inheritdoc />
    protected override DateTimeOffset? GetExpirationTime(TItem item) => item.ExpirationTime;

    /// <inheritdoc />
    protected override TKDataRepeaterBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

    /// <inheritdoc />
    protected override void StoreBlobData(TKDataRepeaterBlobData data) => _blobHelper.StoreBlobData(data);

    internal bool AllowInsertNewInternal()
    {
        try
        {
            if (AllowInsertNew?.Invoke() != true)
            {
                return false;
            }
        }
        catch (Exception) { return false; }

        return true;
    }

    internal bool AllowUpdateExistingInternal()
    {
        try
        {
            if (AllowUpdateExisting?.Invoke() != true)
            {
                return false;
            }
        }
        catch (Exception) { return false; }

        return true;
    }

    /// <summary>
    /// Model stored in blob storage.
    /// </summary>
    public class TKDataRepeaterBlobData : IBufferedBlobDictionaryStorageData
    {
        /// <summary>
        /// All stored items.
        /// </summary>
        public Dictionary<Guid, TItem> Items { get; set; } = new Dictionary<Guid, TItem>();
    }
}
