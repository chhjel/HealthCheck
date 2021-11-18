using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.SecureFileDownload
{
    internal class HCSecureFileDownloadModuleConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Downloads";
        public string ComponentName => "SecureFileDownloadPageComponent";
        public string DefaultRootRouteSegment => "downloads";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}/:id?";
        public List<HealthCheckLinkTagModel> LinkTags => new();
        public List<HealthCheckScriptTagModel> ScriptTags => new();
        public string RawHtml { get; }
    }
}
