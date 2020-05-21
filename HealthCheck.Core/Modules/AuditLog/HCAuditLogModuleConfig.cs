﻿using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.AuditLog
{
    internal class HCAuditLogModuleConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Audit Log";
        public string ComponentName => "AuditLogPageComponent";
        public string DefaultRootRouteSegment => "auditlog";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}";
        public List<HealthCheckLinkTagModel> LinkTags => null;
        public List<HealthCheckScriptTagModel> ScriptTags => null;
    }
}
