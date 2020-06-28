using HealthCheck.Core.Abstractions.Modules;

namespace HealthCheck.Core.Tests.ModuleSystem.Helpers
{
    public class Module_Invalid_NoConfig : HealthCheckModuleBase<ModuleAccessOptions_Valid>
    {
        public override object GetFrontendOptionsObject(HealthCheckModuleContext context) => null;
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => null;
    }
}
