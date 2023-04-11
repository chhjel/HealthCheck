using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.MappedData
{
    internal class TKMappedDataModuleConfig : IToolkitModuleConfig
    {
        public string Name { get; } = "Mapped Data";
        public string ComponentName => "MappedDataPageComponent";
        public string DefaultRootRouteSegment => "mappeddata";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}/:typeId?";
        public List<ToolkitLinkTagModel> LinkTags => new();
        public List<ToolkitScriptTagModel> ScriptTags => new();
        public string RawHtml { get; }
    }
}