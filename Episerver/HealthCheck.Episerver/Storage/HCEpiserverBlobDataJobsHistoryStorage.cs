using EPiServer.Framework.Blobs;
using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Modules.Jobs.Abstractions;
using HealthCheck.Core.Modules.Jobs.Models;
using HealthCheck.Core.Util.Storage;
using HealthCheck.Episerver.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Episerver.Storage
{
    /// <summary>
    /// Stores job history.
    /// </summary>
    public class HCEpiserverBlobDataJobsHistoryStorage
        : HCSingleBufferedListBlobStorageBase<HCEpiserverBlobDataJobsHistoryStorage.HCJobHistoryEntryBlobData, HCJobHistoryEntry>, IHCJobsHistoryStorage
    {
        /// <summary>
        /// Container id used if not overridden.
        /// </summary>
        protected virtual Guid DefaultContainerId => Guid.Parse("8882135f-bc80-40cd-8645-080d8db801d9");

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
        protected override string CacheKey => $"__hc_{ContainerIdWithFallback}";

        private readonly HCEpiserverBlobHelper<HCJobHistoryEntryBlobData> _blobHelper;

        /// <summary>
        /// Stores job history.
        /// </summary>
        public HCEpiserverBlobDataJobsHistoryStorage(IBlobFactory blobFactory, IHCCache cache)
            : base(cache)
        {
            _blobHelper = new HCEpiserverBlobHelper<HCJobHistoryEntryBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
        }

        /// <inheritdoc />
        public Task<HCJobHistoryEntry> InsertHistoryAsync(HCJobHistoryEntry history)
        {
            InsertItemBuffered(history);
            return Task.FromResult(history);
        }

        /// <inheritdoc />
        public Task<HCPagedJobHistoryEntry> GetPagedHistoryAsync(string sourceId, string jobId, int pageIndex, int pageSize)
        {
            var potential = GetItems()
                .Where(x => x.SourceId == sourceId && x.JobId == jobId)
                .OrderByDescending(x => x.EndedAt)
                .ToArray();
            var items = potential
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToList();
            return Task.FromResult(new HCPagedJobHistoryEntry
            {
                Items = items,
                TotalCount = potential.Length
            });
        }

        /// <inheritdoc />
        public Task<List<HCJobHistoryEntry>> GetLatestHistoryPerJobIdAsync()
        {
            var items = GetItems()
                .OrderByDescending(x => x.EndedAt)
                .GroupBy(x => $"{x.SourceId}_{x.JobId}")
                .Select(x => x.First())
                .ToList();
            return Task.FromResult(items);
        }

        /// <inheritdoc />
        public Task<HCJobHistoryEntry> GetHistory(Guid id)
            => Task.FromResult(GetItems().FirstOrDefault(x => x.Id == id));

        /// <inheritdoc />
        public Task DeleteHistoryItemAsync(Guid id)
        {
            RemoveMatching(x => x.Id == id);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task DeleteAllHistoryForJobAsync(string sourceId, string jobId)
        {
            RemoveMatching(x => x.SourceId == sourceId && x.JobId == jobId);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task DeleteAllHistoryAsync()
        {
            RemoveMatching(x => true);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<IEnumerable<HCJobHistoryEntry>> LimitMaxHistoryCountForJob(string sourceId, string jobId, int maxCount)
        {
            var items = GetItems().ToList();
            if (items.Count <= maxCount)
            {
                return Task.FromResult(Enumerable.Empty<HCJobHistoryEntry>());
            }

            var deleted = items
                .OrderByDescending(x => x.EndedAt)
                .Skip(maxCount)
                .ToList();

            var idsToRemove = new HashSet<Guid>(deleted.Select(x => x.Id));
            RemoveMatching(x => idsToRemove.Contains(x.Id));

            return Task.FromResult<IEnumerable<HCJobHistoryEntry>>(deleted);
        }

        /// <inheritdoc />
        protected override HCJobHistoryEntryBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

        /// <inheritdoc />
        protected override void StoreBlobData(HCJobHistoryEntryBlobData data) => _blobHelper.StoreBlobData(data);

        /// <summary>
        /// Model stored in blob storage.
        /// </summary>
        public class HCJobHistoryEntryBlobData : IBufferedBlobListStorageData
        {
            /// <summary>
            /// All stored data.
            /// </summary>
            public List<HCJobHistoryEntry> Items { get; set; } = new List<HCJobHistoryEntry>();
        }
    }
}
