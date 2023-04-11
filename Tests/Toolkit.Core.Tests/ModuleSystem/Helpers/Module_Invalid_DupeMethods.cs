using QoDL.Toolkit.Core.Abstractions.Modules;

namespace QoDL.Toolkit.Core.Tests.ModuleSystem.Helpers
{
    public class Module_Invalid_DupeMethods : ToolkitModuleBase<ModuleAccessOptions_Valid>
    {
        public override object GetFrontendOptionsObject(ToolkitModuleContext context) => null;
        public override IToolkitModuleConfig GetModuleConfig(ToolkitModuleContext context) => new ModuleConfig_Valid();

        [ToolkitModuleMethod]
        public void TestMethod()
        {
            // Method intentionally left empty.
        }

        [ToolkitModuleMethod]
        public void TestMethod(int id)
        {
            // Method intentionally left empty.
        }
    }
}
