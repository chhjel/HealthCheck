using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace QoDL.Toolkit.Module.DataExport
{
    internal class TKDataExportModuleConfig : IToolkitModuleConfig
    {
        public string Name { get; } = "Data Export";
        public string ComponentName => "DataExportPageComponent";
        public string DefaultRootRouteSegment => "dataExport";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}/:streamId?";
        public List<ToolkitLinkTagModel> LinkTags => new();
        public List<ToolkitScriptTagModel> ScriptTags => new();
        public string RawHtml { get; }
    }
}
