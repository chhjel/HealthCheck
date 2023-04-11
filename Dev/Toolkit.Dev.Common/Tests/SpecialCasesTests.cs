using QoDL.Toolkit.Core.Modules.Tests.Attributes;
using QoDL.Toolkit.Core.Modules.Tests.Models;
using QoDL.Toolkit.WebUI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static QoDL.Toolkit.Dev.Common.Tests.OutParameterTests;

namespace QoDL.Toolkit.Dev.Common.Tests
{
    [RuntimeTestClass(
        Name = "Special cases tests",
        DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins,
        GroupName = RuntimeTestConstants.Group.TopGroup,
        UIOrder = 50
    )]
    public class SpecialCasesTests
    {
        public delegate void TestDelegate(string value);

        // todo: json input & specify full typename to serialize to?
        [RuntimeTest]
        public TestResult TestWithInterfaces(IList<string> list, IDisposable disposable, IEnumerable<int> enumerable)
        {
            return TestResult.CreateSuccess("Success hopefully.")
                .AddSerializedData(list)
                .AddSerializedData(disposable)
                .AddSerializedData(enumerable);
        }

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

        [RuntimeTest]
        public TestResult TestWithListOfComplexObjects(List<ComplexDumy> data)
        {
            return TestResult.CreateSuccess("Success hopefully.")
                .AddSerializedData(data);
        }
    }
}
