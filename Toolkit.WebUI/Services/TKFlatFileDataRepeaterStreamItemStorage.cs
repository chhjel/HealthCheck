using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Extensions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Models;
using QoDL.Toolkit.Core.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.WebUI.Services;

/// <summary>
/// Storage for stream items.
/// </summary>
public class TKFlatFileDataRepeaterStreamItemStorage<TItem> : ITKDataRepeaterStreamItemStorage
    where TItem : class, ITKDataRepeaterStreamItem
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
    /// Flatfile storage helper.
    /// </summary>
    protected TKSimpleDataStoreWithId<TItem, Guid> Store { get; }

    /// <summary>
    /// Storage for stream items.
    /// </summary>
    public TKFlatFileDataRepeaterStreamItemStorage(string filepath)
    {
        Store = new TKSimpleDataStoreWithId<TItem, Guid>(
            filepath,
            serializer: new Func<TItem, string>((e) => JsonConvert.SerializeObject(e)),
            deserializer: new Func<string, TItem>((row) => JsonConvert.DeserializeObject<TItem>(row)),
            idSelector: (e) => e.Id,
            idSetter: (e, id) => e.Id = id,
            nextIdFactory: (events, e) => Guid.NewGuid()
        );
    }

    #region ITKDataRepeaterStreamItemStorage implementation
    /// <inheritdoc />
    public Task AddItemAsync(ITKDataRepeaterStreamItem item, object hint = null)
    {
        if (!AllowInsertNewInternal() || item is not TItem convertedItem) return Task.CompletedTask;
        Store.InsertItem(convertedItem);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UpdateItemAsync(ITKDataRepeaterStreamItem item)
    {
        if (!AllowUpdateExisting() || item is not TItem convertedItem) return Task.CompletedTask;
        item.LastUpdatedAt = DateTimeOffset.Now;
        Store.InsertOrUpdateItem(convertedItem);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteItemAsync(Guid id)
    {
        Store.DeleteItem(id);
        return Task.CompletedTask;
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
    public async Task AddItemTagAsync(Guid id, string tag)
    {
        var item = await GetItemAsync(id).ConfigureAwait(false);
        if (item == null || item.Tags?.Contains(tag) == true)
        {
            return;
        }

        item.Tags ??= new HashSet<string>();
        item.Tags.Add(tag);
        await UpdateItemAsync(item).ConfigureAwait(false);
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
    public Task<ITKDataRepeaterStreamItem> GetItemAsync(Guid id)
    {
        var item = Store.GetEnumerable().FirstOrDefault(x => x.Id == id);
        return Task.FromResult(item as ITKDataRepeaterStreamItem);
    }

    /// <inheritdoc />
    public Task<ITKDataRepeaterStreamItem> GetItemByItemIdAsync(string itemId)
    {
        var item = Store.GetEnumerable().FirstOrDefault(x => x.ItemId == itemId);
        return Task.FromResult(item as ITKDataRepeaterStreamItem);
    }

    /// <inheritdoc />
    public Task<TKDataRepeaterStreamItemsPagedModel> GetItemsPagedAsync(TKGetDataRepeaterStreamItemsFilteredRequest model)
    {
        var matches = Store.GetEnumerable()
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
        => Task.FromResult(Store.GetEnumerable().Cast<ITKDataRepeaterStreamItem>());

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
    public Task PerformBatchUpdateAsync(TKDataRepeaterBatchedStorageItemActions actions)
    {
        if (actions.Adds?.Any() == true)
        {
            foreach (var action in actions.Adds)
            {
                Store.InsertItem(action.Item as TItem);
            }
        }

        if (actions.Updates?.Any() == true)
        {
            Store.InsertOrUpdateItems(actions.Updates.Select(x => x.Item).OfType<TItem>());
        }

        if (actions.Deletes?.Any() == true)
        {
            var deleteIds = new HashSet<string>(actions.Deletes.Select(x => x.Item.ItemId));
            Store.DeleteWhere(x => deleteIds.Contains(x.ItemId));
        }
        return Task.CompletedTask;
    }
    #endregion

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
}
