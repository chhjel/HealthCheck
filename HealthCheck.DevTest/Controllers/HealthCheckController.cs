using System.Reflection;
using HealthCheck.Core.TestManagers;
using HealthCheck.DevTest._TestImplementation;

namespace HealthCheck.DevTest.Controllers
{
    public class HealthCheckController : HealthCheckControllerBase
    {
        protected override Assembly GetAssemblyContainingTests() => GetType().Assembly;

        protected override void Config(TestRunner testRunner, TestDiscoverer testDiscoverer)
        {
            testRunner.IncludeExceptionStackTraces = true;
            testDiscoverer.GroupOptions
                .SetOptionsFor(RuntimeTestConstants.Group.Test, uiOrder: -100, iconName: RuntimeTestConstants.Icons.Face);
        }

        protected override object GetRequestAccessRoles()
        {
            var roles = RuntimeTestAccessRole.Guest;

            if (Request.QueryString["webadmin"] != null)
            {
                roles |= RuntimeTestAccessRole.WebAdmins;
            }
            if (Request.QueryString["sysadmin"] != null)
            {
                roles |= RuntimeTestAccessRole.SystemAdmins;
            }

            return roles;
        }
    }
}
