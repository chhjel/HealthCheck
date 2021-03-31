﻿using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;
using System;
using System.Linq.Expressions;

namespace HealthCheck.Dev.Common.Tests
{
    [RuntimeTestClass(
        Name = "Special cases tests",
        DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins,
        GroupName = RuntimeTestConstants.Group.AlmostTopGroup,
        UIOrder = 50
    )]
    public class SpecialCasesTests
    {
        public delegate void TestDelegate(string value);

        [RuntimeTest]
        public TestResult TestWithDelegates(Action action, Func<string> func, TestDelegate deleg, Expression<Func<Action<bool>>> combination)
        {
            var items = new[]
            {
                action?.ToString(),
                func?.ToString(),
                deleg?.ToString(),
                combination?.ToString()
            };
            return TestResult.CreateSuccess("Success hopefully.")
                .AddCodeData(string.Join("\n", items));
        }
    }
}