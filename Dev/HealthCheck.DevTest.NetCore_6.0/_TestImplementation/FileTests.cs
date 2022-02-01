using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Dev.Common;

namespace HealthCheck.DevTest.NetCore_6._0._TestImplementation
{
    [RuntimeTestClass(
        Name = "File tests",
        Description = "Testing file parameters.",
        DefaultRolesWithAccess = RuntimeTestAccessRole.SystemAdmins,
        GroupName = RuntimeTestConstants.Group.AdminStuff,
        UIOrder = 700,
        AllowRunAll = false
    )]
    public class FileTests
    {
        [RuntimeTest]
        public TestResult TestFileParameterType(IFormFile? file = null, List<IFormFile>? files = null)
        {
            return TestResult.CreateSuccess("Ok")
                .AddTextData($"File: {file?.FileName ?? "no file selected"} + {string.Join(", ", files?.Select(x => x?.FileName) ?? Enumerable.Empty<string>())}");
        }
    }
}
