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
        public string RoutePath => "/{0}";
        public List<HealthCheckLinkTagModel> LinkTags => null;
        public List<HealthCheckScriptTagModel> ScriptTags => null;
    }
}
