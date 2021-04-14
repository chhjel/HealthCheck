using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.AccessTokens
{
    internal class HCAccessTokensModuleConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Tokens";
        public string ComponentName => "AccessTokensPageComponent";
        public string DefaultRootRouteSegment => "tokens";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}";
        public List<HealthCheckLinkTagModel> LinkTags => new List<HealthCheckLinkTagModel>();
        public List<HealthCheckScriptTagModel> ScriptTags => new List<HealthCheckScriptTagModel>();
        public string RawHtml { get; }
    }
}
