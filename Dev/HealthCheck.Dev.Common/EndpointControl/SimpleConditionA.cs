using HealthCheck.Core.Attributes;
using HealthCheck.Module.EndpointControl.Abstractions;
using HealthCheck.Module.EndpointControl.Models;
using System;

namespace HealthCheck.Dev.Common.EndpointControl
{
    public class SimpleConditionA : IHCEndpointControlRuleCondition
    {
        public string Name => "Simple condition";
        public string Description => "A test condition.";

        public Type CustomPropertiesModelType => typeof(Properties);

        public bool RequestMatchesCondition(EndpointControlEndpointRequestData requestData, object parameters)
        {
            var p = parameters as Properties;
            return p.Something == true || p.SomeText?.Trim()?.ToLower() == "true";
        }

        public class Properties
        {
            public bool Something { get; set; }

            [HCCustomProperty(UIHints = Core.Models.HCUIHint.CodeArea)]
            public string SomeText { get; set; }
        }
    }
}
