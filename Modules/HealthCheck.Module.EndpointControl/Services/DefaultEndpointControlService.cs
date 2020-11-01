using HealthCheck.Module.EndpointControl.Abstractions;
using HealthCheck.Module.EndpointControl.Models;

namespace HealthCheck.Module.EndpointControl.Services
{
    /// <summary>
    /// Checks if requests to certain endpoints are allowed to execute.
    /// </summary>
    public class DefaultEndpointControlService : IEndpointControlService
    {
        private readonly IEndpointControlRequestHistoryStorage _historicalDataStorage;
        private readonly IEndpointControlEndpointDefinitionStorage _definitionStorage;

        /// <summary>
        /// Checks if requests to certain endpoints are allowed to execute.
        /// </summary>
        public DefaultEndpointControlService(
            IEndpointControlRequestHistoryStorage historicalDataStorage,
            IEndpointControlEndpointDefinitionStorage definitionStorage
        )
        {
            _historicalDataStorage = historicalDataStorage;
            _definitionStorage = definitionStorage;
        }

        /// <summary>
        /// Tracks the given request data and returns true if it is allowed to go through.
        /// </summary>
        public virtual bool HandleRequest(EndpointControlEndpointRequestData requestData)
        {
            var allow = AllowRequest(requestData);

            requestData.WasBlocked = !allow;
            _historicalDataStorage.AddRequest(requestData);

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
        /// Return true to allow the request, or false to block it.
        /// </summary>
        protected virtual bool AllowRequest(EndpointControlEndpointRequestData requestData)
        {
            return !requestData.Url.Contains("pls");
        }

        /* Overview.json:
        {
            requestsPerCountry: {
                "NO": [ { timestamp, wasBlocked }, { timestamp, wasBlocked }, ..],
                "SE": [ { timestamp, wasBlocked }, { timestamp, wasBlocked }, ..]
            }
        }
        */

        // Rules:
        // - Conditions: Multiple AND'ed for targeting.
        //   * Contains/Regex/Match per prop on EndpointRequestData.
        //   * Request count last n minutes.
        // - Actions:
        //   * Block
        //   * Set/exponential delay
        //   * Custom implementations? Log, alert etc?

        // Other features:
        // - Lockdown mode. All and per endpoint w/ end dates. (separate access option for all and single)

    }
}
