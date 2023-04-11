using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Tests.ModuleSystem.Helpers
{
    public class Module_Valid : ToolkitModuleBase<ModuleAccessOptions_Valid>
    {
        public override object GetFrontendOptionsObject(ToolkitModuleContext context) => null;
        public override IToolkitModuleConfig GetModuleConfig(ToolkitModuleContext context) => new ModuleConfig_Valid();

        [ToolkitModuleMethod]
        public void ValidMethod_OpenAccess_NoParameter_NoReturn()
        {
            // Method intentionally left empty.
        }

        [ToolkitModuleMethod]
        public void ValidMethod_OpenAccess_HasParameter_NoReturn(int id)
        {
            // Method intentionally left empty.
        }

        [ToolkitModuleMethod]
        public int ValidMethod_OpenAccess_HasParameter_ReturnsValue(int id) => id;

        [ToolkitModuleMethod]
        public void ValidMethod_OpenAccess_HasParameterAndContext_NoReturn(ToolkitModuleContext context, int id)
        {
            // Method intentionally left empty.
        }

        [ToolkitModuleMethod(ModuleAccessOptions_Valid.Read)]
        public int ValidMethod_RequiresRead_NoParameter_ReturnsValue() => 123;

        [ToolkitModuleMethod(ModuleAccessOptions_Valid.Read | ModuleAccessOptions_Valid.Write)]
        public int ValidMethod_RequiresReadWrite_NoParameter_ReturnsValue() => 123;

        [ToolkitModuleMethod]
        public async Task ValidMethodAsync_OpenAccess_NoParameter_NoReturn() => await Task.Delay(1);

        [ToolkitModuleMethod]
        public async Task<int> ValidMethodAsync_OpenAccess_NoParameter_ReturnsValue() => await Task.FromResult(123);
    }
}
