using System;

namespace QoDL.Toolkit.Core.Tests.Helpers;

[Flags]
public enum AccessRoles
{
    None = 0,

    Guest = 1,
    WebAdmins = 2,
    SystemAdmins = 4,

    Everyone = Guest | WebAdmins | SystemAdmins
}
