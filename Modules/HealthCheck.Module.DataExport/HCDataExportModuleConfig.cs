using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Module.DataExport
{
    internal class HCDataExportModuleConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Data Export";
        public string ComponentName => "DataExportPageComponent";
        public string DefaultRootRouteSegment => "dataExport";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}/:streamId?";
        public List<HealthCheckLinkTagModel> LinkTags => new();
        public List<HealthCheckScriptTagModel> ScriptTags => new();
        public string RawHtml { get; }
    }
}
