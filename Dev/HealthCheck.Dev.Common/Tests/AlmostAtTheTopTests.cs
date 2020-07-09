using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;

namespace HealthCheck.Dev.Common.Tests
{
    [RuntimeTestClass(
        Name = "Almost at the top",
        DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins,
        GroupName = RuntimeTestConstants.Group.AlmostTopGroup,
        UIOrder = 30
    )]
    public class AlmostAtTheTopTests
    {
        [RuntimeTest]
        public TestResult SomeSimpleTest(int number)
        {
            return TestResult.CreateSuccess($"Number {number} is a success!");
        }
    }
}
