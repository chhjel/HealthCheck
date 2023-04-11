using QoDL.Toolkit.Core.Modules.Tests.Attributes;
using QoDL.Toolkit.Core.Modules.Tests.Models;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.WebUI.Extensions;

namespace QoDL.Toolkit.Dev.Common.Tests.Modules;

[RuntimeTestClass(
    Name = "Request data",
    DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins,
    GroupName = RuntimeTestConstants.Group.Modules,
    UIOrder = 30
)]
public class RequestDataTests
{
    [RuntimeTest]
    public TestResult GetRequestData()
    {
        var data = TKRequestData.GetCurrentRequestData();
        return TestResult.CreateSuccess($"Fetched request data!")
            .AddSerializedData(data)
            .AddCodeData(data.ToString(), "ToString()")
            .AddCodeData(data.GetDetails(), "GetDetails()")
            .AddCodeData(data.GetCounters(), "GetCounters()")
            .AddCodeData(data.GetErrors(), "GetErrors()");
    }
}
