﻿using System;

namespace HealthCheck.DevTest._TestImplementation
{
    [Flags]
    public enum RuntimeTestAccessRole
    {
        None = 0,
        Guest = 1,
        WebAdmins = 2,
        SystemAdmins = 4,
        SomethingElse = 8
    }
}