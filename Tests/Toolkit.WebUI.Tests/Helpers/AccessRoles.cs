using System;

namespace QoDL.Toolkit.WebUI.Tests.Helpers
{
    [Flags]
    public enum AccessRoles
    {
        None = 0,

        Guest = 1,
        WebAdmins = 2,
        SystemAdmins = 4,

        Everyone = None | Guest | WebAdmins | SystemAdmins
    }
}
