using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Core.Util;
using HealthCheck.WebUI.Extensions;

namespace HealthCheck.Dev.Common.Tests.Modules
{
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
            var data = HCRequestData.GetCurrentRequestData();
            return TestResult.CreateSuccess($"Fetched request data!")
                .AddSerializedData(data)
                .AddCodeData(data.ToString(), "ToString()")
                .AddCodeData(data.GetDetails(), "GetDetails()")
                .AddCodeData(data.GetCounters(), "GetCounters()")
                .AddCodeData(data.GetErrors(), "GetErrors()");
        }
    }
}
