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
    public class HCEpiserverBlobEndpointControlRuleStorage
        : HCSingleBlobStorageBase<HCEpiserverBlobEndpointControlRuleStorage.HCEndpointControlRuleBlobData>, IEndpointControlRuleStorage
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
        protected override string CacheKey => $"__hc_{ContainerIdWithFallback}";

        private readonly HCEpiserverBlobHelper<HCEndpointControlRuleBlobData> _blobHelper;

        /// <summary>
        /// Create a new <see cref="HCEpiserverBlobEndpointControlRuleStorage"/>.
        /// </summary>
        public HCEpiserverBlobEndpointControlRuleStorage(IBlobFactory blobFactory, IHCCache cache)
            : base(cache)
        {
            _blobHelper = new HCEpiserverBlobHelper<HCEndpointControlRuleBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
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
            SaveBlobData(new HCEndpointControlRuleBlobData());
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
        protected override HCEndpointControlRuleBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

        /// <inheritdoc />
        protected override void StoreBlobData(HCEndpointControlRuleBlobData data) => _blobHelper.StoreBlobData(data);

        /// <summary>
        /// Model stored in blob storage.
        /// </summary>
        public class HCEndpointControlRuleBlobData
        {
            /// <summary>
            /// All stored rules.
            /// </summary>
            public List<EndpointControlRule> Rules { get; set; } = new List<EndpointControlRule>();
        }
    }
}
