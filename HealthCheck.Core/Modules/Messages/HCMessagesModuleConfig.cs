using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Messages
{
    internal class HCMessagesModuleConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Messages";
        public string ComponentName => "MessagesPageComponent";
        public string DefaultRootRouteSegment => "messages";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}/:id?";
        public List<HealthCheckLinkTagModel> LinkTags => new();
        public List<HealthCheckScriptTagModel> ScriptTags => new();
        public string RawHtml { get; }
    }
}
