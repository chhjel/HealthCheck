using QoDL.Toolkit.Core.Modules.Tests.Attributes;
using QoDL.Toolkit.Core.Modules.Tests.Models;
using QoDL.Toolkit.Dev.Common.Dataflow;
using System;
using System.Linq;

namespace QoDL.Toolkit.Dev.Common.Tests.Modules;

[RuntimeTestClass(
    Name = "Dataflow",
    DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins,
    GroupName = RuntimeTestConstants.Group.Modules,
    UIOrder = 30
)]
public class DataflowTests
{
    public static readonly TestStreamA TestStreamA = new();

    [RuntimeTest]
    public TestResult InsertEntries(int count = 10)
    {
        var entriesToInsert = Enumerable.Range(1, count)
            .Select(i => new TestEntry
            {
                Code = $"000{i}-P",
                Name = $"Entry [{DateTimeOffset.Now}]"
            })
            .ToList();

        TestStreamA.InsertEntries(entriesToInsert);
        return TestResult.CreateSuccess($"{count} entries inserted.");
    }
}
