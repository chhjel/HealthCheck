using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.SiteEvents
{
    internal class TKSiteEventsModuleConfig : IToolkitModuleConfig
    {
        public string Name { get; } = "Overview";
        public string ComponentName => "OverviewPageComponent";
        public string DefaultRootRouteSegment => "overview";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}";
        public List<ToolkitLinkTagModel> LinkTags => new();
        public List<ToolkitScriptTagModel> ScriptTags => new();
        public string RawHtml { get; }
    }
}
