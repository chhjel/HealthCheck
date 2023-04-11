using QoDL.Toolkit.Core.Abstractions.Modules;

namespace QoDL.Toolkit.Core.Tests.ModuleSystem.Helpers
{
    public class Module_Invalid_WrongEnum : ToolkitModuleBase<ModuleAccessOptions_Invalid_NotFlags>
    {
        public override object GetFrontendOptionsObject(ToolkitModuleContext context) => null;
        public override IToolkitModuleConfig GetModuleConfig(ToolkitModuleContext context) => new ModuleConfig_Valid();
    }
}
