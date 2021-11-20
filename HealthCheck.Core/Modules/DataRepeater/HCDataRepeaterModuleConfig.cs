using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.DataRepeater
{
    internal class HCDataRepeaterModuleConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Data Repeater";
        public string ComponentName => "DataRepeaterPageComponent";
        public string DefaultRootRouteSegment => "dataRepeater";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}/:streamId?/:itemId?";
        public List<HealthCheckLinkTagModel> LinkTags => new();
        public List<HealthCheckScriptTagModel> ScriptTags => new();
        public string RawHtml { get; }
    }
}
