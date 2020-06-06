using HealthCheck.Core.Abstractions.Modules;

namespace HealthCheck.Core.Tests.ModuleSystem.Helpers
{
    public class Module_Invalid_DupeMethods : HealthCheckModuleBase<ModuleAccessOptions_Valid>
    {
        public override object GetFrontendOptionsObject(HealthCheckModuleContext context) => null;
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => new ModuleConfig_Valid();

        [HealthCheckModuleMethod]
        public void TestMethod() { }

        [HealthCheckModuleMethod]
#pragma warning disable IDE0060 // Remove unused parameter
        public void TestMethod(int id) { }
#pragma warning restore IDE0060 // Remove unused parameter
    }
}
