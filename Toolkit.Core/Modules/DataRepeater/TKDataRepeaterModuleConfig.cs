using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.DataRepeater
{
    internal class TKDataRepeaterModuleConfig : IToolkitModuleConfig
    {
        public string Name { get; } = "Data Repeater";
        public string ComponentName => "DataRepeaterPageComponent";
        public string DefaultRootRouteSegment => "dataRepeater";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}/:streamId?/:itemId?";
        public List<ToolkitLinkTagModel> LinkTags => new();
        public List<ToolkitScriptTagModel> ScriptTags => new();
        public string RawHtml { get; }
    }
}
