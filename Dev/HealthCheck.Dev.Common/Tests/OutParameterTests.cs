using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;

namespace HealthCheck.Dev.Common.Tests
{
    [RuntimeTestClass(
        Name = "Out parameters tests",
        DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins,
        GroupName = RuntimeTestConstants.Group.AlmostTopGroup,
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
                Value = "Complex"
            };
            return TestResult.CreateSuccess($"Success hopefully. [{somethingStr}, {somethingInt}]");
        }

        public class ComplexDumy {
            public string Value { get; set; }
        }
    }
}
