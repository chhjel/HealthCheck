using QoDL.Toolkit.Core.Modules.Tests.Attributes;
using QoDL.Toolkit.Core.Modules.Tests.Models;
using QoDL.Toolkit.Dev.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.DevTest.NetCore_6._0._TestImplementation;

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
    public TestResult TestFileParameterType(IFormFile? file = null, List<IFormFile>? files = null)
    {
        return TestResult.CreateSuccess("Ok")
            .AddTextData($"File: {file?.FileName ?? "no file selected"} + {string.Join(", ", files?.Select(x => x?.FileName) ?? Enumerable.Empty<string>())}");
    }
}
