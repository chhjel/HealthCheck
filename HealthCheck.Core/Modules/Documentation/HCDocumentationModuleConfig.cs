using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Documentation
{
    internal class HCDocumentationModuleConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Documentation";
        public string ComponentName => "DocumentationPageComponent";
        public string DefaultRootRouteSegment => "documentation";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}/:title?";
        public List<HealthCheckLinkTagModel> LinkTags => new();
        public List<HealthCheckScriptTagModel> ScriptTags => new();
        public string RawHtml { get; }
    }
}
