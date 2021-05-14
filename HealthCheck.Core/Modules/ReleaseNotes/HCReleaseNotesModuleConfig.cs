using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.ReleaseNotes
{
    internal class HCReleaseNotesModuleConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Release Notes";
        public string ComponentName => "ReleaseNotesPageComponent";
        public string DefaultRootRouteSegment => "releasenotes";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}";
        public List<HealthCheckLinkTagModel> LinkTags => new();
        public List<HealthCheckScriptTagModel> ScriptTags => new();
        public string RawHtml { get; }
    }
}
