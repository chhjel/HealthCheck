using HealthCheck.Module.EndpointControl.Abstractions;
using HealthCheck.Module.EndpointControl.Models;
using System;
using System.Collections.Generic;
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
        private readonly List<IEndpointControlRequestResult> _customBlockedResults = new();

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
        /// Get any defined custom blocked results.
        /// </summary>
        public IEnumerable<IEndpointControlRequestResult> GetCustomBlockedResults() => _customBlockedResults;

        /// <summary>
        /// Add a custom result that can be selected in the UI and used when request are blocked.
        /// </summary>
        public DefaultEndpointControlService AddCustomBlockedResult(IEndpointControlRequestResult result)
        {
            _customBlockedResults.Add(result);
            return this;
        }

        /// <summary>
        /// Tracks the given request data and returns true if it is allowed to go through.
        /// </summary>
        public virtual EndpointControlHandledRequestResult HandleRequest(EndpointControlEndpointRequestData requestData, bool storeData)
        {
            var result = new EndpointControlHandledRequestResult();

            if (!IsEnabledInternal())
            {
                result.WasDecidedToAllowRequest = true;
                return result;
            }

            var allow = AllowRequest(requestData, out EndpointControlRule blockingRule);
            result.BlockingRule = blockingRule;
            result.CustomBlockedResult = _customBlockedResults?.FirstOrDefault(x => x.Id == blockingRule?.BlockResultTypeId);

            requestData.WasBlocked = !allow && result.CustomBlockedResult?.CountAsBlockedRequest != false;
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

            result.WasDecidedToAllowRequest = allow;
            return result;
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
    }
}
