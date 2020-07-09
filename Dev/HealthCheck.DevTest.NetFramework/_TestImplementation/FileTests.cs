using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Dev.Common;
using System.Web;

namespace HealthCheck.DevTest._TestImplementation
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
        public TestResult TestFileParameterType(HttpPostedFileBase file = null)
        {
            return TestResult.CreateSuccess("Ok")
                .AddTextData($"File: {file?.FileName ?? "no file selected"}");
        }
    }
}
