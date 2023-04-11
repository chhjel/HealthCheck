using QoDL.Toolkit.Core.Modules.Tests.Attributes;
using QoDL.Toolkit.Core.Modules.Tests.Models;

namespace QoDL.Toolkit.Dev.Common.Tests
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
