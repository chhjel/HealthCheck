using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;

namespace HealthCheck.Dev.Common.Tests
{
    [RuntimeTestClass(
        Name = "Failed test class",
        DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins,
        GroupName = RuntimeTestConstants.Group.TopGroup,
        UIOrder = 50
    )]
    public class FailedTestClass
    {
        public FailedTestClass(object obj) {}

        [RuntimeTest]
        public TestResult FailingTestBecauseOfClassErrors()
        {
            return TestResult.CreateSuccess("Success perhaps.");
        }
    }
}
