using System;

namespace HealthCheck.Dev.Common
{
    [Flags]
    public enum RuntimeTestAccessRole
    {
        None = 0,
        Guest = 1,
        WebAdmins = 2,
        SystemAdmins = 4,
        SomethingElse = 8,
        API = 16,
        Testing = 32,
        QuerystringTest = 64
    }
}