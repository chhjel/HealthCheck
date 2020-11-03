using HealthCheck.Module.EndpointControl.Abstractions;

namespace HealthCheck.Module.EndpointControl.Module
{
    /// <summary>
    /// Options for <see cref="HCEndpointControlModule"/>.
    /// </summary>
    public class HCEndpointControlModuleOptions
    {
        /// <summary>
        /// Checks if requests to certain endpoints are allowed to execute.
        /// </summary>
        public IEndpointControlService EndpointControlService { get; set; }

        /// <summary>
        /// Rule storage implementation that edited rules will be stored in.
        /// </summary>
        public IEndpointControlRuleStorage RuleStorage { get; set; }

        /// <summary>
        /// Definition storage implementation that definitions will be stored in.
        /// </summary>
        public IEndpointControlEndpointDefinitionStorage DefinitionStorage { get; set; }

        /// <summary>
        /// Storage implementation that handles historical data.
        /// </summary>
        public IEndpointControlRequestHistoryStorage HistoryStorage { get; set; }
    }
}
