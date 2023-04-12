using System;

namespace QoDL.Toolkit.WebUI.Tests.ModuleSystem.Helpers
{
    [Flags]
    public enum ModuleAccessOptions_Valid
    {
        None = 0,
        Read = 1,
        Write = 2,
        Execute = 4,
        DeleteEverything = 8
    }
}
