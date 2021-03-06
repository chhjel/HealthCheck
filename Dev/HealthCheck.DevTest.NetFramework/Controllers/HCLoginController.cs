using HealthCheck.WebUI.Abstractions;
using HealthCheck.WebUI.Models;

namespace HealthCheck.DevTest.Controllers
{
    public class HCLoginController : HealthCheckLoginControllerBase
    {
        protected override HCIntegratedLoginResult HandleLoginRequest(HCIntegratedLoginRequest request)
        {
            var success = request.Username == "root"
                && request.Password == "toor"
                && request.TwoFactorCode == "otp";

            return new HCIntegratedLoginResult
            {
                Success = success,
                ErrorMessage = $"Wrong username or password, try again or <b>give up</b>. <a href=\"https://www.google.com\">Help!</a>",
                ShowErrorAsHtml = true
            };
        }
    }
}
