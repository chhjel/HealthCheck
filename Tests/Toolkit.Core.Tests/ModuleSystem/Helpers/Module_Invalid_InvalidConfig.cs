using QoDL.Toolkit.Core.Abstractions.Modules;

namespace QoDL.Toolkit.Core.Tests.ModuleSystem.Helpers;

public class Module_Invalid_NoConfig : ToolkitModuleBase<ModuleAccessOptions_Valid>
{
    public override object GetFrontendOptionsObject(ToolkitModuleContext context) => null;
    public override IToolkitModuleConfig GetModuleConfig(ToolkitModuleContext context) => null;
}
