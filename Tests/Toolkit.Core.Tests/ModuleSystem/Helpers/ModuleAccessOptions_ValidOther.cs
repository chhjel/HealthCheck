using System;

namespace QoDL.Toolkit.Core.Tests.ModuleSystem.Helpers;

[Flags]
public enum ModuleAccessOptions_ValidOther
{
    None = 0,
    Read = 1,
    Write = 2,
    Execute = 4,
    DeleteEverything = 8
}
