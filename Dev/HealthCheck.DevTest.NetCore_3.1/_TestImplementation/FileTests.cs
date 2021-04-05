using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Dev.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;

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
        public TestResult TestFileParameterType(IFormFile file = null, List<IFormFile> files = null)
        {
            return TestResult.CreateSuccess("Ok")
                .AddTextData($"File: {file?.FileName ?? "no file selected"} + {string.Join(", ", files?.Select(x => x?.FileName))}");
        }
    }
}
