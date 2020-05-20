using HealthCheck.Core.Attributes;
using HealthCheck.Core.Modules.Tests.Models;

namespace HealthCheck.DevTest._TestImplementation.Tests
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
