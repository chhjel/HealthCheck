using QoDL.Toolkit.Core.Modules.Tests.Attributes;
using QoDL.Toolkit.Core.Modules.Tests.Models;

namespace QoDL.Toolkit.Dev.Common.Tests
{
    [RuntimeTestClass(
        Name = "Out parameters tests",
        DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins,
        GroupName = RuntimeTestConstants.Group.TopGroup,
        UIOrder = 101
    )]
    public class OutParameterTests
    {
        [RuntimeTest]
        public TestResult TestWithSomeComplexParameters(string somethingStr, out bool boolTest, int somethingInt, out ComplexDumy complexTest, out int intTest)
        {
            boolTest = true;
            intTest = 123;
            complexTest = new ComplexDumy
            {
                Value = "Complex" + somethingStr
            };
            return TestResult.CreateSuccess($"Success hopefully. [{somethingStr}, {somethingInt}]");
        }

        [RuntimeTest]
        public TestResult TestWithRefsParameters(string somethingStr, ref bool boolTest, int somethingInt, ref ComplexDumy complexTestRef, ref int intTest)
        {
            boolTest = true;
            intTest = 123;
            complexTestRef = new ComplexDumy
            {
                Value = "Complex" + somethingStr
            };
            return TestResult.CreateSuccess($"Success hopefully. [{somethingStr}, {somethingInt}]");
        }

        public class ComplexDumy {
            public string Value { get; set; }
        }
    }
}
