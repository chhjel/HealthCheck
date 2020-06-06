using HealthCheck.Core.Abstractions.Modules;

namespace HealthCheck.Core.Tests.ModuleSystem.Helpers
{
    public class Module_Invalid_WrongEnum : HealthCheckModuleBase<ModuleAccessOptions_Invalid_NotFlags>
    {
        public override object GetFrontendOptionsObject(HealthCheckModuleContext context) => null;
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => new ModuleConfig_Valid();
    }
}
