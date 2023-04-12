using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Extensions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Models;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Storage;

/// <summary>
/// Stores datarepeater items in memory only.
/// <para>Mostly for dev usage.</para>
/// </summary>
public class MemoryDataRepeaterStreamItemStorage : ITKDataRepeaterStreamItemStorage
{
    private static readonly Dictionary<string, ITKDataRepeaterStreamItem> _items = new();
    private readonly string _prefix;

    /// <summary>
    /// Stores datarepeater items in memory only.
    /// <para>Mostly for dev usage.</para>
    /// </summary>
    /// <param name="prefix">Prefix to store items by, set to something unique per stream using this storage.</param>
    public MemoryDataRepeaterStreamItemStorage(string prefix)
    {
        _prefix = prefix;
    }

    /// <inheritdoc />
    public Task AddItemAsync(ITKDataRepeaterStreamItem item, object hint = null)
    {
        lock (_items)
        {
            _items[$"{_prefix}_{item.Id}"] = item;
            return Task.CompletedTask;
        }
    }

    /// <inheritdoc />
    public Task UpdateItemAsync(ITKDataRepeaterStreamItem item)
    {
        lock (_items)
        {
            item.LastUpdatedAt = DateTimeOffset.Now;
            _items[$"{_prefix}_{item.Id}"] = item;
            return Task.CompletedTask;
        }
    }

    /// <inheritdoc />
    public async Task AddItemTagAsync(Guid id, string tag)
    {
        var item = await GetItemAsync(id);
        if (item == null || item.Tags?.Contains(tag) == true)
        {
            return;
        }

        item.Tags ??= new HashSet<string>();
        item.Tags.Add(tag);
        await UpdateItemAsync(item);
    }

    /// <inheritdoc />
    public async Task AddItemTagsAsync(Guid id, params string[] tags)
    {
        var item = await GetItemAsync(id);
        if (item == null || tags?.Any() != true || tags.All(t => item.Tags?.Contains(t) == true))
        {
            return;
        }

        item.Tags ??= new HashSet<string>();
        foreach (var tag in tags)
        {
            item.Tags.Add(tag);
        }
        await UpdateItemAsync(item);
    }

    /// <inheritdoc />
    public async Task RemoveItemTagAsync(Guid id, string tag)
    {
        var item = await GetItemAsync(id);
        if (item == null || item.Tags?.Contains(tag) != true)
        {
            return;
        }

        item.Tags ??= new HashSet<string>();
        item.Tags.Add(tag);
        await UpdateItemAsync(item);
    }

    /// <inheritdoc />
    public async Task RemoveItemTagsAsync(Guid id, params string[] tags)
    {
        var item = await GetItemAsync(id);
        if (item != null && tags?.Any() == true && tags.Any(t => item.Tags?.Contains(t) == true))
        {
            item.Tags ??= new HashSet<string>();
            foreach (var tag in tags)
            {
                item.Tags.Remove(tag);
            }
            await UpdateItemAsync(item);
        }
    }

    /// <inheritdoc />
    public async Task RemoveAllItemTagsAsync(Guid id)
    {
        var item = await GetItemAsync(id);
        if (item == null || item.Tags?.Any() != true)
        {
            return;
        }

        item.Tags ??= new HashSet<string>();
        item.Tags.Clear();
        await UpdateItemAsync(item);
    }

    /// <inheritdoc />
    public async Task SetAllowItemRetryAsync(Guid id, bool allow)
    {
        var item = await GetItemAsync(id);
        if (item == null || item.AllowRetry == allow)
        {
            return;
        }

        item.AllowRetry = allow;
        await UpdateItemAsync(item);
    }

    /// <inheritdoc />
    public async Task SetItemExpirationTimeAsync(Guid id, DateTimeOffset? time)
    {
        var item = await GetItemAsync(id);
        if (item == null || item.ExpirationTime == time)
        {
            return;
        }

        item.ExpirationTime = time;
        await UpdateItemAsync(item);
    }

    /// <inheritdoc />
    public async Task SetForcedItemStatusAsync(Guid id, TKDataRepeaterStreamItemStatus? status, Maybe<DateTimeOffset?> expirationTime = null, string logMessage = null, string error = null)
    {
        var item = await GetItemAsync(id);
        if (item == null)
        {
            return;
        }

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

        await UpdateItemAsync(item);
    }

    /// <inheritdoc />
    public async Task AddItemLogMessageAsync(Guid id, string logMessage)
    {
        var item = await GetItemAsync(id);
        if (item == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(logMessage))
        {
            item.AddLogMessage(logMessage);
            await UpdateItemAsync(item);
        }
    }

    /// <inheritdoc />
    public async Task SetItemErrorAsync(Guid id, string error)
    {
        var item = await GetItemAsync(id);
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
            await UpdateItemAsync(item);
        }
    }

    /// <inheritdoc />
    public Task DeleteItemAsync(Guid id)
    {
        lock (_items)
        {
            _items.Remove($"{_prefix}_{id}");
            return Task.CompletedTask;
        }
    }

    /// <inheritdoc />
    public Task<ITKDataRepeaterStreamItem> GetItemAsync(Guid id)
    {
        lock (_items)
        {
            var item = _items.ContainsKey($"{_prefix}_{id}") ? _items[$"{_prefix}_{id}"] : null;
            return Task.FromResult(item);
        }
    }

    /// <inheritdoc />
    public Task<ITKDataRepeaterStreamItem> GetItemByItemIdAsync(string itemId)
    {
        lock (_items)
        {
            var item = _items.Where(x => x.Key.StartsWith($"{_prefix}"))
                .FirstOrDefault(x => x.Value.ItemId == itemId).Value;
            return Task.FromResult(item);
        }
    }

    /// <inheritdoc />
    public Task<TKDataRepeaterStreamItemsPagedModel> GetItemsPagedAsync(TKGetDataRepeaterStreamItemsFilteredRequest model)
    {
        lock (_items)
        {
            var matches = _items.Where(x => x.Key.StartsWith($"{_prefix}_"))
                .Select(x => x.Value)
                .Where(x =>
                    (string.IsNullOrWhiteSpace(model.Filter)
                    || x.ItemId?.ToLower()?.Contains(model.Filter?.ToLower()) == true
                    || x.Summary?.ToLower()?.Contains(model.Filter?.ToLower()) == true)
                    && (model.Tags?.Any() != true || model.Tags.All(t => x.Tags?.Any(tt => tt?.ToLower() == t.ToLower()) == true) == true)
                    && (model.RetryAllowed == null || x.AllowRetry == model.RetryAllowed))
                .OrderByDescending(x => x.InsertedAt)
                .ToArray();
            var items = matches
                .Skip(model.PageIndex * model.PageSize)
                .Take(model.PageSize);
            var result = new TKDataRepeaterStreamItemsPagedModel
            {
                TotalCount = matches.Count(),
                Items = items
            };
            return Task.FromResult(result);
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<ITKDataRepeaterStreamItem>> GetAllItemsAsync()
    {
        lock (_items)
        {
            return Task.FromResult(_items.Select(x => x.Value));
        }
    }

    /// <inheritdoc />
    public Task PerformBatchUpdateAsync(TKDataRepeaterBatchedStorageItemActions actions)
    {
        lock (_items)
        {
            if (actions.Adds?.Any() == true)
            {
                foreach (var action in actions.Adds)
                {
                    AddItemAsync(action.Item, action.Hint);
                }
            }

            if (actions.Updates?.Any() == true)
            {
                foreach (var action in actions.Updates)
                {
                    UpdateItemAsync(action.Item);
                }
            }

            if (actions.Deletes?.Any() == true)
            {
                foreach (var action in actions.Deletes)
                {
                    DeleteItemAsync(action.Item.Id);
                }
            }
        }
        return Task.CompletedTask;
    }
}
