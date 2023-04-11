using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace QoDL.Toolkit.Module.DynamicCodeExecution.Module
{
    internal class TKDynamicCodeExecutionModuleConfig : IToolkitModuleConfig
    {
        public string Name { get; } = "Code";
        public string ComponentName => "DynamicCodeExecutionPageComponent";
        public string DefaultRootRouteSegment => "code";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}/:id?";
        public List<ToolkitLinkTagModel> LinkTags => new();
        public List<ToolkitScriptTagModel> ScriptTags => new();
        public string RawHtml { get; }
    }
}
