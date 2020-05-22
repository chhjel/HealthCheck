using HealthCheck.Core.Abstractions.Modules;

namespace HealthCheck.Core.Tests.ModuleSystem.Helpers
{
    public class Module_Invalid_ConfigException : HealthCheckModuleBase<ModuleAccessOptions_Valid>
    {
        public override object GetFrontendOptionsObject(ModuleAccessOptions_Valid access) => null;
        public override IHealthCheckModuleConfig GetModuleConfig(ModuleAccessOptions_Valid access) => new ModuleConfig_Invalid_Exception();
    }
}
