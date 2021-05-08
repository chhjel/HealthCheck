using EPiServer.Framework.Blobs;
using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Util.Storage;
using HealthCheck.Episerver.Utils;
using HealthCheck.Module.EndpointControl.Abstractions;
using HealthCheck.Module.EndpointControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Episerver.EndpointControl
{
    /// <summary>
    /// Stores rule data in episerver blobstorage.
    /// </summary>
    public class HCEpiserverBlobEndpointControlEndpointDefinitionStorage
        : HCSingleBlobStorageBase<HCEpiserverBlobEndpointControlEndpointDefinitionStorage.HCEndpointControlDefinitionBlobData>,
        IEndpointControlEndpointDefinitionStorage
    {
        /// <summary>
        /// Container id used if not overridden.
        /// </summary>
        protected virtual Guid DefaultContainerId => Guid.Parse("823e4a10-00ce-4da0-a0f3-16ff544dd2c7");

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

        private readonly HCEpiserverBlobHelper<HCEndpointControlDefinitionBlobData> _blobHelper;

        /// <summary>
        /// Create a new <see cref="HCEpiserverBlobEndpointControlRuleStorage"/>.
        /// </summary>
        public HCEpiserverBlobEndpointControlEndpointDefinitionStorage(IBlobFactory blobFactory, IHCCache cache)
            : base(cache)
        {
            _blobHelper = new HCEpiserverBlobHelper<HCEndpointControlDefinitionBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
        }

        /// <inheritdoc />
        public bool HasDefinitionFor(string endpointId)
        {
            var data = GetBlobData();
            return data?.Definitions?.Any(x => x.EndpointId == endpointId) == true;
        }

        /// <inheritdoc />
        public void StoreDefinition(EndpointControlEndpointDefinition definition)
        {
            var data = GetBlobData();
            data.Definitions.Add(definition);
            SaveBlobData(data);
        }

        /// <inheritdoc />
        public IEnumerable<EndpointControlEndpointDefinition> GetDefinitions()
            => GetBlobData().Definitions ?? Enumerable.Empty<EndpointControlEndpointDefinition>();

        /// <inheritdoc />
        public Task ClearAllDefinitions()
        {
            SaveBlobData(new HCEndpointControlDefinitionBlobData());
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task DeleteDefinition(string endpointId)
        {
            var data = GetBlobData();
            if (data?.Definitions?.Any(x => x.EndpointId == endpointId) == true)
            {
                data.Definitions.RemoveAll(x => x.EndpointId == endpointId);
                SaveBlobData(data);
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        protected override HCEndpointControlDefinitionBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

        /// <inheritdoc />
        protected override void StoreBlobData(HCEndpointControlDefinitionBlobData data) => _blobHelper.StoreBlobData(data);

        /// <summary>
        /// Model stored in blob storage.
        /// </summary>
        public class HCEndpointControlDefinitionBlobData
        {
            /// <summary>
            /// All stored rules.
            /// </summary>
            public List<EndpointControlEndpointDefinition> Definitions { get; set; } = new List<EndpointControlEndpointDefinition>();
        }
    }
}
