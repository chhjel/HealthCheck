using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Module.EndpointControl.Abstractions;
using QoDL.Toolkit.Module.EndpointControl.Models;
using System;

namespace QoDL.Toolkit.Dev.Common.EndpointControl
{
    public class SimpleConditionA : ITKEndpointControlRuleCondition
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

            [TKCustomProperty(UIHints = Core.Models.TKUIHint.CodeArea)]
            public string SomeText { get; set; }
        }
    }
}
