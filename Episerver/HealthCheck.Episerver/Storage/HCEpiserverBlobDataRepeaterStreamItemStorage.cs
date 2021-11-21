using EPiServer.Framework.Blobs;
using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using HealthCheck.Core.Util.Storage;
using HealthCheck.Episerver.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Episerver.Storage
{
    /// <summary>
    /// Stores datarepeater items.
    /// </summary>
    /// <example>
    /// <![CDATA[
    ///     public class MyData {}
    ///     public class MyStreamItem : HCDefaultDataRepeaterStreamItem<MyData, MyStreamItem> {}
    ///     public class MyStorage : HCEpiserverBlobDataRepeaterStreamItemStorage<MyStreamItem> {}
    /// ]]>
    /// </example>
    public abstract class HCEpiserverBlobDataRepeaterStreamItemStorage<TItem>
        : HCSingleBufferedDictionaryBlobStorageBase<HCEpiserverBlobDataRepeaterStreamItemStorage<TItem>.HCDataRepeaterBlobData, TItem, Guid>, IHCDataRepeaterStreamItemStorage
        where TItem : IHCDataRepeaterStreamItem
    {
        /// <summary>
        /// Defaults to the default provider if null.
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Blob container id.
        /// </summary>
        protected abstract Guid ContainerId { get; }

        /// <inheritdoc />
        protected override string CacheKey => $"__hc_{ContainerId}";

        private readonly HCEpiserverBlobHelper<HCDataRepeaterBlobData> _blobHelper;

        /// <summary>
        /// Stores datarepeater items.
        /// </summary>
        public HCEpiserverBlobDataRepeaterStreamItemStorage(IBlobFactory blobFactory, IHCCache cache)
            : base(cache)
        {
            _blobHelper = new HCEpiserverBlobHelper<HCDataRepeaterBlobData>(blobFactory, () => ContainerId, () => ProviderName);
            BlobUpdateBufferDuration = TimeSpan.FromSeconds(30);
            SupportsMaxItemAge = true;
        }

        #region IHCDataRepeaterStreamItemStorage Implementation
        /// <inheritdoc />
        public Task StoreItemAsync(IHCDataRepeaterStreamItem item, object hint = null)
        {
            InsertItemBuffered((TItem)item, item.Id);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task UpdateItemAsync(IHCDataRepeaterStreamItem item)
        {
            InsertItemBuffered((TItem)item, item.Id, isUpdate: true);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<IHCDataRepeaterStreamItem> GetItemAsync(Guid id)
        {
            var item = GetItems().FirstOrDefault(x => x.Id == id);
            return Task.FromResult(item as IHCDataRepeaterStreamItem);
        }

        /// <inheritdoc />
        public Task<IHCDataRepeaterStreamItem> GetItemByItemIdAsync(string itemId)
        {
            var item = GetItems().FirstOrDefault(x => x.ItemId == itemId);
            return Task.FromResult(item as IHCDataRepeaterStreamItem);
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
        public Task<HCDataRepeaterStreamItemsPagedModel> GetItemsPagedAsync(HCGetDataRepeaterStreamItemsFilteredRequest model)
        {
            var items = GetItems()
                .Where(x => 
                    (string.IsNullOrWhiteSpace(model.ItemId) || x.ItemId?.Contains(model.ItemId) == true)
                    && (model.Tags?.Any() != true || x.Tags?.Any(t => model.Tags?.Contains(t) == true) == true))
                .OfType<IHCDataRepeaterStreamItem>()
                .Skip(model.PageIndex * model.PageSize)
                .Take(model.PageSize);
            var result = new HCDataRepeaterStreamItemsPagedModel
            {
                TotalCount = GetItems().Count(),
                Items = items
            };
            return Task.FromResult(result);
        }
        #endregion

        /// <inheritdoc />
        protected override DateTimeOffset GetItemTimestamp(TItem item) => item.InsertedAt;

        /// <inheritdoc />
        protected override HCDataRepeaterBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

        /// <inheritdoc />
        protected override void StoreBlobData(HCDataRepeaterBlobData data) => _blobHelper.StoreBlobData(data);

        /// <summary>
        /// Model stored in blob storage.
        /// </summary>
        public class HCDataRepeaterBlobData : IBufferedBlobDictionaryStorageData
        {
            /// <summary>
            /// All stored items.
            /// </summary>
            public Dictionary<Guid, TItem> Items { get; set; } = new Dictionary<Guid, TItem>();
        }
    }
}
