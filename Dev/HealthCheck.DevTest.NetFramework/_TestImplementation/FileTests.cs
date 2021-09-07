using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Dev.Common;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public TestResult TestDownloadResult()
            => TestResult.CreateSuccess($"Hopefully success?")
                //.SetCleanMode()
                .AddFileDownload("Test1", "Some file.txt", "Description of the file here. Also with type.", type: "url")
                .AddFileDownload("Test2", "Some file.pdf")
                .AddFileDownload("ascii", "Ascii file.txt")
                .AddFileDownload("404", "missing.txt", "This one should give a 404.", title: "Even a title here")
                .AddFileDownload(Guid.NewGuid().ToString(), "random_guid.txt");

        [RuntimeTest]
        public TestResult TestFileParameterType(HttpPostedFileBase file = null, List<HttpPostedFileBase> files = null)
        {
            return TestResult.CreateSuccess("Ok")
                .AddTextData($"File: {file?.FileName ?? "no file selected"} + {string.Join(", ", files?.Select(x => x?.FileName))}");
        }

        [RuntimeTest]
        public TestResult TestByteArrParameterType(byte[] bytes)
        {
            return TestResult.CreateSuccess($"Ok, byte count: {bytes?.Length}");
        }
    }
}
