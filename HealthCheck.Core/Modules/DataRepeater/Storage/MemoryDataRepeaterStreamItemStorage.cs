using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Extensions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.DataRepeater.Storage
{
    /// <summary>
    /// Stores datarepeater items in memory only.
    /// <para>Mostly for dev usage.</para>
    /// </summary>
    public class MemoryDataRepeaterStreamItemStorage : IHCDataRepeaterStreamItemStorage
    {
        private static readonly Dictionary<string, IHCDataRepeaterStreamItem> _items = new();
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
        public Task AddItemAsync(IHCDataRepeaterStreamItem item, object hint = null)
        {
            lock(_items)
            {
                _items[$"{_prefix}_{item.Id}"] = item;
                return Task.CompletedTask;
            }
        }

        /// <inheritdoc />
        public Task UpdateItemAsync(IHCDataRepeaterStreamItem item)
        {
            lock (_items)
            {
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
            foreach(var tag in tags)
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
        public async Task SetForcedItemStatusAsync(Guid id, HCDataRepeaterStreamItemStatus? status, Maybe<DateTimeOffset?> expirationTime = null, string logMessage = null)
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
            await UpdateItemAsync(item);
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
        public Task<IHCDataRepeaterStreamItem> GetItemAsync(Guid id)
        {
            lock (_items)
            {
                var item = _items.ContainsKey($"{_prefix}_{id}") ? _items[$"{_prefix}_{id}"] : null;
                return Task.FromResult(item);
            }
        }

        /// <inheritdoc />
        public Task<IHCDataRepeaterStreamItem> GetItemByItemIdAsync(string itemId)
        {
            lock (_items)
            {
                var item = _items.Where(x => x.Key.StartsWith($"{_prefix}"))
                    .FirstOrDefault(x => x.Value.ItemId == itemId).Value;
                return Task.FromResult(item);
            }
        }

        /// <inheritdoc />
        public Task<HCDataRepeaterStreamItemsPagedModel> GetItemsPagedAsync(HCGetDataRepeaterStreamItemsFilteredRequest model)
        {
            lock (_items)
            {
                var matches = _items.Where(x => x.Key.StartsWith($"{_prefix}_"))
                    .Select(x => x.Value)
                    .Where(x =>
                        (string.IsNullOrWhiteSpace(model.Filter)
                        || x.ItemId?.ToLower()?.Contains(model.Filter?.ToLower()) == true
                        || x.Summary?.ToLower()?.Contains(model.Filter?.ToLower()) == true)
                        && (model.Tags?.Any() != true || x.Tags?.Any(t => model.Tags?.Any(tt => tt?.ToLower() == t.ToLower()) == true) == true)
                        && (model.RetryAllowed == null || x.AllowRetry == model.RetryAllowed))
                    .OrderByDescending(x => x.InsertedAt)
                    .ToArray();
                var items = matches
                    .Skip(model.PageIndex * model.PageSize)
                    .Take(model.PageSize);
                var result = new HCDataRepeaterStreamItemsPagedModel
                {
                    TotalCount = matches.Count(),
                    Items = items
                };
                return Task.FromResult(result);
            }
        }
    }
}
