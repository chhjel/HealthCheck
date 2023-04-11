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
public class TKEpiserverBlobEndpointControlRuleStorage
    : TKSingleBlobStorageBase<TKEpiserverBlobEndpointControlRuleStorage.TKEndpointControlRuleBlobData>, IEndpointControlRuleStorage
{
    /// <summary>
    /// Container id used if not overridden.
    /// </summary>
    protected virtual Guid DefaultContainerId => Guid.Parse("84a083ad-04da-4612-b37d-7194bc52bfef");

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

    private readonly TKEpiserverBlobHelper<TKEndpointControlRuleBlobData> _blobHelper;

    /// <summary>
    /// Create a new <see cref="TKEpiserverBlobEndpointControlRuleStorage"/>.
    /// </summary>
    public TKEpiserverBlobEndpointControlRuleStorage(IBlobFactory blobFactory, ITKCache cache)
        : base(cache)
    {
        _blobHelper = new TKEpiserverBlobHelper<TKEndpointControlRuleBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
    }

    /// <inheritdoc />
    public void DeleteRule(Guid ruleId)
    {
        var data = GetBlobData();
        if (data?.Rules?.Any(x => x.Id == ruleId) == true)
        {
            data.Rules.RemoveAll(x => x.Id == ruleId);
            SaveBlobData(data);
        }
    }

    /// <inheritdoc />
    public Task DeleteRules()
    {
        SaveBlobData(new TKEndpointControlRuleBlobData());
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public EndpointControlRule GetRule(Guid id)
    {
        var data = GetBlobData();
        return data?.Rules?.FirstOrDefault(x => x.Id == id);
    }

    /// <inheritdoc />
    public IEnumerable<EndpointControlRule> GetRules()
        => GetBlobData().Rules ?? Enumerable.Empty<EndpointControlRule>();

    /// <inheritdoc />
    public EndpointControlRule InsertRule(EndpointControlRule rule)
    {
        var data = GetBlobData();

        rule.Id = Guid.NewGuid();
        data.Rules.Add(rule);
        SaveBlobData(data);

        return rule;
    }

    /// <inheritdoc />
    public EndpointControlRule UpdateRule(EndpointControlRule rule)
    {
        var data = GetBlobData();
        data.Rules.RemoveAll(x => x.Id == rule?.Id);
        data.Rules.Add(rule);
        SaveBlobData(data);
        return rule;
    }

    /// <inheritdoc />
    protected override TKEndpointControlRuleBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

    /// <inheritdoc />
    protected override void StoreBlobData(TKEndpointControlRuleBlobData data) => _blobHelper.StoreBlobData(data);

    /// <summary>
    /// Model stored in blob storage.
    /// </summary>
    public class TKEndpointControlRuleBlobData
    {
        /// <summary>
        /// All stored rules.
        /// </summary>
        public List<EndpointControlRule> Rules { get; set; } = new List<EndpointControlRule>();
    }
}
