using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.MappedData
{
    internal class HCMappedDataModuleConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Mapped Data";
        public string ComponentName => "MappedDataPageComponent";
        public string DefaultRootRouteSegment => "mappeddata";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}/:typeId?";
        public List<HealthCheckLinkTagModel> LinkTags => new();
        public List<HealthCheckScriptTagModel> ScriptTags => new();
        public string RawHtml { get; }
    }
}