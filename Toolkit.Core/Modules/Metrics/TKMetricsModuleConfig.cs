using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Metrics
{
    internal class TKMetricsModuleConfig : IToolkitModuleConfig
    {
        public string Name { get; } = "Metrics";
        public string ComponentName => "MetricsPageComponent";
        public string DefaultRootRouteSegment => "metrics";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}";
        public List<ToolkitLinkTagModel> LinkTags => new();
        public List<ToolkitScriptTagModel> ScriptTags => new();
        public string RawHtml { get; }
    }
}
