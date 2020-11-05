using HealthCheck.Module.EndpointControl.Abstractions;
using HealthCheck.Module.EndpointControl.Models;
using System;
using System.Linq;

namespace HealthCheck.Module.EndpointControl.Services
{
    /// <summary>
    /// Checks if requests to certain endpoints are allowed to execute.
    /// </summary>
    public class DefaultEndpointControlService : IEndpointControlService
    {
        /// <summary>
        /// If disabled the service will ignore any attempts to register requests, and nothing will be blocked or stored.
        /// <para>Enabled by default. Null value/exception = false.</para>
        /// </summary>
        public Func<bool> IsEnabled { get; set; } = () => true;

        private readonly IEndpointControlRequestHistoryStorage _historicalDataStorage;
        private readonly IEndpointControlEndpointDefinitionStorage _definitionStorage;
        private readonly IEndpointControlRuleStorage _ruleStorage;

        /// <summary>
        /// Checks if requests to certain endpoints are allowed to execute.
        /// </summary>
        public DefaultEndpointControlService(
            IEndpointControlRequestHistoryStorage historicalDataStorage,
            IEndpointControlEndpointDefinitionStorage definitionStorage,
            IEndpointControlRuleStorage ruleStorage
        )
        {
            _historicalDataStorage = historicalDataStorage;
            _definitionStorage = definitionStorage;
            _ruleStorage = ruleStorage;
        }

        /// <summary>
        /// Tracks the given request data and returns true if it is allowed to go through.
        /// </summary>
        public virtual bool HandleRequest(EndpointControlEndpointRequestData requestData, bool storeData)
        {
            if (!IsEnabledInternal())
            {
                return true;
            }

            var allow = AllowRequest(requestData, out EndpointControlRule blockingRule);

            requestData.WasBlocked = !allow;
            if (!allow)
            {
                requestData.BlockingRuleId = blockingRule?.Id;
            }

            if (storeData)
            {
                StoreHistoricalRequestData(requestData);
            }

            if (!_definitionStorage.HasDefinitionFor(requestData.EndpointId))
            {
                _definitionStorage.StoreDefinition(new EndpointControlEndpointDefinition
                {
                    EndpointId = requestData.EndpointId,
                    HttpMethod = requestData.HttpMethod,
                    ControllerName = requestData.ControllerName,
                    ActionName = requestData.ActionName
                });
            }

            return allow;
        }

        /// <summary>
        /// Use to manually store request data.
        /// <para>Optionally manually invoked from <c>EndpointControlUtils.CountCurrentRequest()</c>.</para>
        /// </summary>
        public void StoreHistoricalRequestData(EndpointControlEndpointRequestData requestData)
        {
            if (!IsEnabledInternal())
            {
                return;
            }
            _historicalDataStorage.AddRequest(requestData);
        }

        /// <summary>
        /// Return true to allow the request, or false to block it.
        /// </summary>
        protected virtual bool AllowRequest(EndpointControlEndpointRequestData requestData, out EndpointControlRule blockingRule)
        {
            blockingRule = _ruleStorage.GetRules()
                .FirstOrDefault(rule => rule.ShouldBlockRequest(requestData,
                    endpointRequestCountGetter: (date) => _historicalDataStorage.GetEndpointRequestCountSince(requestData.UserLocationId, requestData.EndpointId, date),
                    totalRequestCountGetter: (date) => _historicalDataStorage.GetTotalRequestCountSince(requestData.UserLocationId, date)
                ));

            return blockingRule == null;
        }

        internal bool IsEnabledInternal()
        {
            try
            {
                if (IsEnabled?.Invoke() != true)
                {
                    return false;
                }
            }
            catch (Exception) { return false; }

            return true;
        }

        // ToDo:
        // Rules:
        // - Actions:
        //   * Block
        //   * Set/exponential delay
        //   * Custom implementations? Log, alert etc?

        // Other features:
        // - Add data filter/modifier to service, to allow for easy url stripping etc.
        // - Lockdown mode. All and per endpoint w/ end dates. (separate access option for all and single)
        // - Identity aliases?
        // - Overview w/ requests per country, requires external ip to country lookup
    }
}
