using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Module.DynamicCodeExecution.Module
{
    internal class HCDynamicCodeExecutionModuleConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Code";
        public string ComponentName => "DynamicCodeExecutionPageComponent";
        public string DefaultRootRouteSegment => "code";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}/:id?";
        public List<HealthCheckLinkTagModel> LinkTags => new List<HealthCheckLinkTagModel>();
        public List<HealthCheckScriptTagModel> ScriptTags => new List<HealthCheckScriptTagModel>();
    }
}
