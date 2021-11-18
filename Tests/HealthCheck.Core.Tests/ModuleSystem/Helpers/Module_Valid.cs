using HealthCheck.Core.Abstractions.Modules;
using System.Threading.Tasks;

namespace HealthCheck.Core.Tests.ModuleSystem.Helpers
{
    public class Module_Valid : HealthCheckModuleBase<ModuleAccessOptions_Valid>
    {
        public override object GetFrontendOptionsObject(HealthCheckModuleContext context) => null;
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => new ModuleConfig_Valid();

        [HealthCheckModuleMethod]
        public void ValidMethod_OpenAccess_NoParameter_NoReturn()
        {
            // Method intentionally left empty.
        }

        [HealthCheckModuleMethod]
        public void ValidMethod_OpenAccess_HasParameter_NoReturn(int id)
        {
            // Method intentionally left empty.
        }

        [HealthCheckModuleMethod]
        public int ValidMethod_OpenAccess_HasParameter_ReturnsValue(int id) => id;

        [HealthCheckModuleMethod]
        public void ValidMethod_OpenAccess_HasParameterAndContext_NoReturn(HealthCheckModuleContext context, int id)
        {
            // Method intentionally left empty.
        }

        [HealthCheckModuleMethod(ModuleAccessOptions_Valid.Read)]
        public int ValidMethod_RequiresRead_NoParameter_ReturnsValue() => 123;

        [HealthCheckModuleMethod(ModuleAccessOptions_Valid.Read | ModuleAccessOptions_Valid.Write)]
        public int ValidMethod_RequiresReadWrite_NoParameter_ReturnsValue() => 123;

        [HealthCheckModuleMethod]
        public async Task ValidMethodAsync_OpenAccess_NoParameter_NoReturn() => await Task.Delay(1);

        [HealthCheckModuleMethod]
        public async Task<int> ValidMethodAsync_OpenAccess_NoParameter_ReturnsValue() => await Task.FromResult(123);
    }
}
