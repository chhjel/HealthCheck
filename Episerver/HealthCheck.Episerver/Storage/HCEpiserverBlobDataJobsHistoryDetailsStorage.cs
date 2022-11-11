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
    /// Stores job history details.
    /// </summary>
    public class HCEpiserverBlobDataJobsHistoryDetailsStorage
        : HCSingleBufferedListBlobStorageBase<HCEpiserverBlobDataJobsHistoryDetailsStorage.HCJobHistoryDetailEntryBlobData, HCJobHistoryDetailEntry>, IHCJobsHistoryDetailsStorage
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
        protected override string CacheKey => $"__hc_{ContainerIdWithFallback}";

        private readonly HCEpiserverBlobHelper<HCJobHistoryDetailEntryBlobData> _blobHelper;

        /// <summary>
        /// Stores job history details.
        /// </summary>
        public HCEpiserverBlobDataJobsHistoryDetailsStorage(IBlobFactory blobFactory, IHCCache cache)
            : base(cache)
        {
            _blobHelper = new HCEpiserverBlobHelper<HCJobHistoryDetailEntryBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
            MaxItemCount = 100;
        }

        /// <inheritdoc />
        public Task<HCJobHistoryDetailEntry> InsertDetailAsync(HCJobHistoryDetailEntry detail)
        {
            InsertItemBuffered(detail);
            return Task.FromResult(detail);
        }

        /// <inheritdoc />
        public Task<HCJobHistoryDetailEntry> GetDetailAsync(Guid id)
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
        protected override HCJobHistoryDetailEntryBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

        /// <inheritdoc />
        protected override void StoreBlobData(HCJobHistoryDetailEntryBlobData data) => _blobHelper.StoreBlobData(data);

        /// <summary>
        /// Model stored in blob storage.
        /// </summary>
        public class HCJobHistoryDetailEntryBlobData : IBufferedBlobListStorageData
        {
            /// <summary>
            /// All stored data.
            /// </summary>
            public List<HCJobHistoryDetailEntry> Items { get; set; } = new List<HCJobHistoryDetailEntry>();
        }
    }
}
