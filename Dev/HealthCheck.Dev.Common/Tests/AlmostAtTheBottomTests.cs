using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;

namespace HealthCheck.Dev.Common.Tests
{
    [RuntimeTestClass(
        Name = "Almost at the bottom",
        DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins,
        GroupName = RuntimeTestConstants.Group.AlmostBottomGroup,
        UIOrder = 30
    )]
    public class AlmostAtTheBottomTests
    {
        [RuntimeTest]
        public TestResult SomeSimpleTest(int number)
        {
            return TestResult.CreateSuccess($"Number {number} is a success!");
        }
    }
}
