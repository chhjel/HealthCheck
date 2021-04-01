using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Dev.Common.Tests
{
    [RuntimeTestClass(
        Name = "Custom ref type tests",
        DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins,
        GroupName = RuntimeTestConstants.Group.AlmostTopGroup,
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
        public TestResult ListCustoms(CustomReferenceType item, List<CustomReferenceType> listRefs)
        {
            return TestResult.CreateSuccess($"Selected: [{item?.Title}] + [{string.Join(", ", listRefs.Select(x => $"{x?.Title}/{x?.Id}"))}]");
        }
    }

    public class CustomReferenceType
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }
}
