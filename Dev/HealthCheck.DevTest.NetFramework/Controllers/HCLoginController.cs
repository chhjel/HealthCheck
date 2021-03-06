using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.TFA;

namespace HealthCheck.DevTest.Controllers
{
    public class HCLoginController : HealthCheckLogin2FAControllerBase
    {
        protected override HCIntegratedLoginResult HandleLoginRequest(HCIntegratedLoginRequest request)
        {
            var userPassOk = request.Username == "root"
                && request.Password == "toor";

            if (!userPassOk)
            {
                return HCIntegratedLoginResult.CreateError("Wrong username or password, try again or <b>give up</b>.<br /><a href=\"https://www.google.com\">Help me!</a>", true);
            }

            var otpOk = Validate2FACode("J5V5XFSQCT2TDG6AZIQ46TTEAXGU7GCW", request.TwoFactorCode);
            if (!otpOk)
            {
                return HCIntegratedLoginResult.CreateError("Two-factor code was wrong, try again.");
            }

            return HCIntegratedLoginResult.CreateSuccess();
        }
    }
}
