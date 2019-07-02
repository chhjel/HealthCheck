using HealthCheck.DevTest._TestImplementation;

namespace HealthCheck.DevTest.Controllers
{
    public class HealthCheckController : HealthCheckControllerBase
    {
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
