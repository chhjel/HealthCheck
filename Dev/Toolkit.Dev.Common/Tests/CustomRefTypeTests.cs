using QoDL.Toolkit.Core.Modules.Tests.Attributes;
using QoDL.Toolkit.Core.Modules.Tests.Models;
using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Dev.Common.Tests;

[RuntimeTestClass(
    Name = "Custom ref type tests",
    DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins,
    GroupName = RuntimeTestConstants.Group.TopGroup,
    UIOrder = 70
)]
public class CustomRefTypeTests
{
    [RuntimeTest]
    public TestResult SimpleCustoms(CustomReferenceType item1, CustomReferenceType item2)
    {
        return TestResult.CreateSuccess($"Selected: [{item1?.Title}/{item1?.Id}|{item2?.Title}/{item2?.Id}]");
    }

    [RuntimeTest]
    public TestResult ListCustoms(CustomReferenceType item, List<CustomReferenceType> listRefs,
        List<CustomReferenceType> listRefs2, CustomReferenceType item2,
        List<CustomReferenceType> listRefs3, List<CustomReferenceType> listRefs4,
        CustomReferenceType item3, CustomReferenceType item4)
    {
        return TestResult.CreateSuccess($"Selected: [{item?.Title}] + [{string.Join(", ", listRefs.Select(x => $"{x?.Title}/{x?.Id}"))}]");
    }
}

public class CustomReferenceType
{
    public int Id { get; set; }
    public string Title { get; set; }
}
