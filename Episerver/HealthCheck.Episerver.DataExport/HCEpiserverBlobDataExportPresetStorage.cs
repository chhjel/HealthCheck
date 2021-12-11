using EPiServer.Framework.Blobs;
using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Util.Storage;
using HealthCheck.Episerver.Utils;
using HealthCheck.Module.DataExport.Abstractions;
using HealthCheck.Module.DataExport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Episerver.DataExport
{
    /// <summary>
    /// Stores and retrieves presets using episerver blobstorage.
    /// </summary>
    public class HCEpiserverBlobDataExportPresetStorage
        : HCSingleBlobStorageBase<HCEpiserverBlobDataExportPresetStorage.HCDataExportPresetBlobData>, IHCDataExportPresetStorage
    {
        /// <summary>
        /// Container id used if not overridden.
        /// </summary>
        protected virtual Guid DefaultContainerId => Guid.Parse("8849c8de-4278-423f-bfc4-3264ea3170a2");

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

        private readonly HCEpiserverBlobHelper<HCDataExportPresetBlobData> _blobHelper;

        /// <summary>
        /// Create a new <see cref="HCEpiserverBlobDataExportPresetStorage"/>.
        /// </summary>
        public HCEpiserverBlobDataExportPresetStorage(IBlobFactory blobFactory, IHCCache cache)
            : base(cache)
        {
            _blobHelper = new HCEpiserverBlobHelper<HCDataExportPresetBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
        }

        #region IHCDataExportPresetStorage Implementation
        /// <inheritdoc />
        public Task<IEnumerable<HCDataExportStreamQueryPreset>> GetStreamQueryPresetsAsync(string streamId)
            => Task.FromResult(GetBlobData().Presets.Where(x => x.StreamId == streamId));

        /// <inheritdoc />
        public Task<HCDataExportStreamQueryPreset> GetStreamQueryPresetAsync(Guid id)
            => Task.FromResult(GetBlobData().Presets.FirstOrDefault(x => x.Id == id));

        /// <inheritdoc />
        public Task DeleteStreamQueryPresetAsync(Guid id)
        {
            var data = GetBlobData();
            if (data?.Presets?.Any(x => x.Id == id) == true)
            {
                data.Presets.RemoveAll(x => x.Id == id);
                SaveBlobData(data);
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<HCDataExportStreamQueryPreset> SaveStreamQueryPresetAsync(HCDataExportStreamQueryPreset preset)
        {
            var data = GetBlobData();

            if (preset.Id == Guid.Empty)
            {
                preset.Id = Guid.NewGuid();
            }
            
            var existingIndex = data.Presets?.FindIndex(x => x.Id == preset.Id) ?? -1;
            if (existingIndex == -1)
            {
                data.Presets.Add(preset);
            }
            else
            {
                data.Presets[existingIndex] = preset;
            }

            SaveBlobData(data);
            return Task.FromResult(preset);
        }
        #endregion

        /// <inheritdoc />
        protected override HCDataExportPresetBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

        /// <inheritdoc />
        protected override void StoreBlobData(HCDataExportPresetBlobData data) => _blobHelper.StoreBlobData(data);

        /// <summary>
        /// Model stored in blob storage.
        /// </summary>
        public class HCDataExportPresetBlobData
        {
            /// <summary>
            /// Stored presets.
            /// </summary>
            public List<HCDataExportStreamQueryPreset> Presets { get; set; } = new List<HCDataExportStreamQueryPreset>();
        }
    }
}
