using EPiServer.Framework.Blobs;
using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Util.Storage;
using HealthCheck.Episerver.Utils;
using HealthCheck.Module.DynamicCodeExecution.Abstractions;
using HealthCheck.Module.DynamicCodeExecution.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Episerver.DynamicCodeExecution
{
    /// <summary>
    /// Stores and retrieves <see cref="DynamicCodeScript"/>s using episerver blobstorage.
    /// </summary>
    public class HCEpiserverBlobDynamicCodeScriptStorage
        : HCSingleBlobStorageBase<HCEpiserverBlobDynamicCodeScriptStorage.HCDynamicCodeExecutionBlobData>, IDynamicCodeScriptStorage
    {
        /// <summary>
        /// Container id used if not overridden.
        /// </summary>
        protected virtual Guid DefaultContainerId => Guid.Parse("81b6186e-5909-4148-b527-f6e2be64c759");

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

        private readonly HCEpiserverBlobHelper<HCDynamicCodeExecutionBlobData> _blobHelper;

        /// <summary>
        /// Create a new <see cref="HCEpiserverBlobDynamicCodeScriptStorage"/>.
        /// </summary>
        public HCEpiserverBlobDynamicCodeScriptStorage(IBlobFactory blobFactory, IHCCache cache)
            : base(cache)
        {
            _blobHelper = new HCEpiserverBlobHelper<HCDynamicCodeExecutionBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
        }

        /// <summary>
        /// Get all stored scripts.
        /// </summary>
        public async Task<List<DynamicCodeScript>> GetAllScripts()
            => await Task.FromResult(GetBlobData().Scripts);

        /// <summary>
        /// Get a single stored script.
        /// </summary>
        public async Task<DynamicCodeScript> GetScript(Guid id)
            => await Task.FromResult(GetBlobData().Scripts.FirstOrDefault(x => x.Id == id));

        /// <summary>
        /// Deletes a single stored script.
        /// </summary>
        public async Task<bool> DeleteScript(Guid id)
        {
            var data = GetBlobData();
            if (data?.Scripts?.Any(x => x.Id == id) == true)
            {
                data.Scripts.RemoveAll(x => x.Id == id);
                SaveBlobData(data);
            }
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Save or create the given script and return the script with any changes.
        /// </summary>
        public async Task<DynamicCodeScript> SaveScript(DynamicCodeScript script)
        {
            var data = GetBlobData();

            var scriptToSave = (script.Id == Guid.Empty)
                ? default
                : data.Scripts?.FirstOrDefault(x => x.Id == script.Id);

            if (scriptToSave == null)
            {
                scriptToSave = script;
                scriptToSave.Id = (scriptToSave.Id == Guid.Empty) ? Guid.NewGuid() : scriptToSave.Id;
                data.Scripts.Add(scriptToSave);
            }

            scriptToSave.Title = script.Title;
            scriptToSave.Code = script.Code;
            
            SaveBlobData(data);
            return await Task.FromResult(scriptToSave);
        }

        /// <inheritdoc />
        protected override HCDynamicCodeExecutionBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

        /// <inheritdoc />
        protected override void StoreBlobData(HCDynamicCodeExecutionBlobData data) => _blobHelper.StoreBlobData(data);

        /// <summary>
        /// Model stored in blob storage.
        /// </summary>
        public class HCDynamicCodeExecutionBlobData
        {
            /// <summary>
            /// Stored scripts.
            /// </summary>
            public List<DynamicCodeScript> Scripts { get; set; } = new List<DynamicCodeScript>();
        }
    }
}
