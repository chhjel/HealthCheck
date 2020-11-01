#if NETFULL
using HealthCheck.Core.Util;
using HealthCheck.Module.EndpointControl.Abstractions;
using HealthCheck.Module.EndpointControl.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Module.EndpointControl.Storage
{
    /// <summary>
    /// Stores rule data on disk.
    /// </summary>
    public class FlatFileEndpointControlRuleStorage : IEndpointControlRuleStorage
    {
        private SimpleDataStoreWithId<EndpointControlRule, Guid> Store { get; }
        private List<EndpointControlRule> _memoryCache = null;

        /// <summary>
        /// Create a new <see cref="FlatFileEndpointControlRuleStorage"/> with the given file path.
        /// </summary>
        /// <param name="filepath">Filepath to where the data will be stored.</param>
        public FlatFileEndpointControlRuleStorage(string filepath)
        {
            Store = new SimpleDataStoreWithId<EndpointControlRule, Guid>(
                filepath,
                serializer: new Func<EndpointControlRule, string>((e) => JsonConvert.SerializeObject(e)),
                deserializer: new Func<string, EndpointControlRule>((row) => JsonConvert.DeserializeObject<EndpointControlRule>(row)),
                idSelector: (x) => x.Id,
                idSetter: (x, id) => x.Id = id,
                nextIdFactory: (all, x) => Guid.NewGuid()
            );
        }

        /// <summary>
        /// Delete rule for the given rule id.
        /// </summary>
        public void DeleteRule(Guid ruleId)
        {
            Store.DeleteItem(ruleId);
        }

        /// <summary>
        /// Clear all rules.
        /// </summary>
        public async Task DeleteRules()
        {
            _memoryCache.Clear();
            await Store.ClearDataAsync();
        }

        /// <summary>
        /// Get all rules.
        /// </summary>
        public IEnumerable<EndpointControlRule> GetRules()
        {
            EnsureMemoryCache();
            return _memoryCache;
        }

        /// <summary>
        /// Insert the given rule.
        /// </summary>
        public EndpointControlRule InsertRule(EndpointControlRule rule)
        {
            EnsureMemoryCache();
            if (!_memoryCache.Any(x => x.Id == rule.Id))
            {
                _memoryCache.Add(rule);
            }

            return Store.InsertItem(rule);
        }

        /// <summary>
        /// Updates the given rule.
        /// </summary>
        public EndpointControlRule UpdateRule(EndpointControlRule rule)
        {
            EnsureMemoryCache();
            _memoryCache.RemoveAll(x => x.Id == rule.Id);
            _memoryCache.Add(rule);

            return Store.InsertOrUpdateItem(rule);
        }

        private void EnsureMemoryCache()
        {
            if (_memoryCache == null)
            {
                _memoryCache = Store.GetEnumerable().ToList();
            }
        }
    }
}
#endif
