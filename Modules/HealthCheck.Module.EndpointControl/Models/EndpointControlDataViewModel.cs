using System.Collections.Generic;

namespace HealthCheck.Module.EndpointControl.Models
{
    /// <summary>
    /// Contains initially loaded data on endpoint control page.
    /// </summary>
    public class EndpointControlDataViewModel
    {
        /// <summary>
        /// All stored rules.
        /// </summary>
        public IEnumerable<EndpointControlRule> Rules { get; set; }

        /// <summary>
        /// All stored endpoint definitions.
        /// </summary>
        public IEnumerable<EndpointControlEndpointDefinition> EndpointDefinitions { get; set; }
    }
}
