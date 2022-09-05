using HealthCheck.Module.EndpointControl.Models;
using System;

namespace HealthCheck.Module.EndpointControl.Abstractions
{
    /// <summary>
    /// Condition that must succeed for a rule to take effect.
    /// </summary>
    public interface IHCEndpointControlRuleCondition
    {
        /// <summary>
        /// Name of this condition to show in the UI.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Description of this condition to show in the UI.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Type of custom properties model if any.
        /// </summary>
        Type CustomPropertiesModelType { get; }

        /// <summary>
        /// Checks if the given request data matches the condition.
        /// </summary>
        bool RequestMatchesCondition(EndpointControlEndpointRequestData requestData, object parameters);
    }
}
