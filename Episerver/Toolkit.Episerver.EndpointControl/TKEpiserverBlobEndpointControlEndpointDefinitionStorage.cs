using EPiServer.Framework.Blobs;
using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Util.Storage;
using QoDL.Toolkit.Episerver.Utils;
using QoDL.Toolkit.Module.EndpointControl.Abstractions;
using QoDL.Toolkit.Module.EndpointControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Episerver.EndpointControl;

/// <summary>
/// Stores rule data in episerver blobstorage.
/// </summary>
public class TKEpiserverBlobEndpointControlEndpointDefinitionStorage
    : TKSingleBlobStorageBase<TKEpiserverBlobEndpointControlEndpointDefinitionStorage.TKEndpointControlDefinitionBlobData>,
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
    protected override string CacheKey => $"__tk_{ContainerIdWithFallback}";

    private readonly TKEpiserverBlobHelper<TKEndpointControlDefinitionBlobData> _blobHelper;

    /// <summary>
    /// Create a new <see cref="TKEpiserverBlobEndpointControlRuleStorage"/>.
    /// </summary>
    public TKEpiserverBlobEndpointControlEndpointDefinitionStorage(IBlobFactory blobFactory, ITKCache cache)
        : base(cache)
    {
        _blobHelper = new TKEpiserverBlobHelper<TKEndpointControlDefinitionBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
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
        SaveBlobData(new TKEndpointControlDefinitionBlobData());
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
    protected override TKEndpointControlDefinitionBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

    /// <inheritdoc />
    protected override void StoreBlobData(TKEndpointControlDefinitionBlobData data) => _blobHelper.StoreBlobData(data);

    /// <summary>
    /// Model stored in blob storage.
    /// </summary>
    public class TKEndpointControlDefinitionBlobData
    {
        /// <summary>
        /// All stored rules.
        /// </summary>
        public List<EndpointControlEndpointDefinition> Definitions { get; set; } = new List<EndpointControlEndpointDefinition>();
    }
}
