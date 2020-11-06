using HealthCheck.WebUI.Abstractions;
using HealthCheck.WebUI.Models;

namespace HealthCheck.DevTest.Controllers
{
    public class HCLoginController : HealthCheckLoginControllerBase
    {
        protected override HCIntegratedLoginResult HandleLoginRequest(HCIntegratedLoginRequest request)
        {
            var success = request.Username == "root" && request.Password == "toor";
            return new HCIntegratedLoginResult
            {
                Success = success,
                ErrorMessage = $"Wrong username or password, try again or give up."
            };
        }
    }
}
