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

        /// <summary>
        /// Max number of requests to retrieve and show in the UI.
        /// <para>Also used for graphs.</para>
        /// <para>Defaults to 100.</para>
        /// </summary>
        public int MaxLatestRequestsToShow { get; set; } = 100;

        /// <summary>
        /// Max number of simple request data to retrieve and show in the UI.
        /// <para>Used for graphs.</para>
        /// <para>Defaults to 1000.</para>
        /// </summary>
        public int MaxLatestSimpleRequestDataToShow { get; set; } = 1000;
    }
}
