using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Messages
{
    internal class TKMessagesModuleConfig : IToolkitModuleConfig
    {
        public string Name { get; } = "Messages";
        public string ComponentName => "MessagesPageComponent";
        public string DefaultRootRouteSegment => "messages";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}/:id?";
        public List<ToolkitLinkTagModel> LinkTags => new();
        public List<ToolkitScriptTagModel> ScriptTags => new();
        public string RawHtml { get; }
    }
}
