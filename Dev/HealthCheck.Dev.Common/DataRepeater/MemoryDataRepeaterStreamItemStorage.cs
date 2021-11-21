using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.DataRepeater
{
    public class MemoryDataRepeaterStreamItemStorage : IHCDataRepeaterStreamItemStorage
    {
        private static Dictionary<string, IHCDataRepeaterStreamItem> _items = new Dictionary<string, IHCDataRepeaterStreamItem>();
        private readonly string _prefix;

        public MemoryDataRepeaterStreamItemStorage(string prefix)
        {
            _prefix = prefix;
        }

        public Task StoreItemAsync(IHCDataRepeaterStreamItem item, object hint = null)
        {
            _items[$"{_prefix}_{item.Id}"] = item;
            return Task.CompletedTask;
        }

        public Task UpdateItemAsync(IHCDataRepeaterStreamItem item)
        {
            _items[$"{_prefix}_{item.Id}"] = item;
            return Task.CompletedTask;
        }

        public async Task AddItemTagAsync(Guid id, string tag)
        {
            var item = await GetItemAsync(id);
            if (item == null || item.Tags?.Contains(tag) == true)
            {
                return;
            }

            item.Tags = item.Tags ?? new HashSet<string>();
            item.Tags.Add(tag);
            await UpdateItemAsync(item);
        }

        public async Task RemoveItemTagAsync(Guid id, string tag)
        {
            var item = await GetItemAsync(id);
            if (item == null || item.Tags?.Contains(tag) != true)
            {
                return;
            }

            item.Tags = item.Tags ?? new HashSet<string>();
            item.Tags.Add(tag);
            await UpdateItemAsync(item);
        }

        public async Task RemoveAllItemTagsAsync(Guid id)
        {
            var item = await GetItemAsync(id);
            if (item == null || item.Tags?.Any() != true)
            {
                return;
            }

            item.Tags = item.Tags ?? new HashSet<string>();
            item.Tags.Clear();
            await UpdateItemAsync(item);
        }

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

        public Task DeleteItemAsync(Guid id)
        {
            _items.Remove($"{_prefix}_{id}");
            return Task.CompletedTask;
        }

        public Task<IHCDataRepeaterStreamItem> GetItemAsync(Guid id)
        {
            var item = _items.ContainsKey($"{_prefix}_{id}") ? _items[$"{_prefix}_{id}"] : null;
            return Task.FromResult(item);
        }

        public Task<IHCDataRepeaterStreamItem> GetItemByItemIdAsync(string itemId)
        {
            var item = _items.Where(x => x.Key.StartsWith($"{_prefix}"))
                .FirstOrDefault(x => x.Value.ItemId == itemId).Value;
            return Task.FromResult(item);
        }

        public Task<HCDataRepeaterStreamItemsPagedModel> GetItemsPagedAsync(HCGetDataRepeaterStreamItemsFilteredRequest model)
        {
            var relevant = _items.Where(x => x.Key.StartsWith($"{_prefix}_"));
            var items = relevant
                .Select(x => x.Value)
                .Where(x => x.ItemId.Contains(model.ItemId) || x.Tags.Any(t => model.Tags.Contains(t)))
                .Skip(model.PageIndex * model.PageSize)
                .Take(model.PageSize);
            var result = new HCDataRepeaterStreamItemsPagedModel
            {
                TotalCount = relevant.Count(),
                Items = items
            };
            return Task.FromResult(result);
        }
    }
}
