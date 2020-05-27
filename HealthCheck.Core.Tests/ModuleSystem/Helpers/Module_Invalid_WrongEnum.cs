using HealthCheck.Core.Abstractions.Modules;

namespace HealthCheck.Core.Tests.ModuleSystem.Helpers
{
    public class Module_Invalid_WrongEnum : HealthCheckModuleBase<ModuleAccessOptions_Invalid_NotFlags>
    {
        public override object GetFrontendOptionsObject(ModuleAccessOptions_Invalid_NotFlags access) => null;
        public override IHealthCheckModuleConfig GetModuleConfig(ModuleAccessOptions_Invalid_NotFlags access) => new ModuleConfig_Valid();
    }
}
