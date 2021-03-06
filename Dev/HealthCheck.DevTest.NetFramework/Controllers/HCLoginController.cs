using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.TFA;
using HealthCheck.WebUI.TFA.Util;

namespace HealthCheck.DevTest.Controllers
{
    public class HCLoginController : HealthCheckLogin2FAControllerBase
    {
        private const string DummySecret = "J5V5XFSQCT2TDG6AZIQ46TTEAXGU7GCW";

        protected override HCIntegratedLoginResult HandleLoginRequest(HCIntegratedLoginRequest request)
        {
            var userPassOk = request.Username == "root"
                && request.Password == "toor";

            if (!userPassOk)
            {
                return HCIntegratedLoginResult.CreateError("Wrong username or password, try again or <b>give up</b>.<br /><a href=\"https://www.google.com\">Help me!</a>", true);
            }

            var otpOk = Validate2FATotpCode(DummySecret, request.TwoFactorCode);
            var sessionCodeValid = ValidateSession2FACode(request.Username, request.TwoFactorCode);

            if (!otpOk && !sessionCodeValid)
            {
                return HCIntegratedLoginResult.CreateError("Two-factor code was wrong, try again.");
            }

            return HCIntegratedLoginResult.CreateSuccess();
        }

        protected override HCIntegratedLogin2FACodeRequestResult Handle2FACodeRequest(HCIntegratedLoginRequest2FACodeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                return HCIntegratedLogin2FACodeRequestResult.CreateError("You must enter your username first.");
            }

            var code = HealthCheck2FAUtil.GenerateCode(DummySecret);
            var sessionCode = CreateSession2FACode(request.Username);
            return HCIntegratedLogin2FACodeRequestResult.CreateSuccess($"TOTP code is <b>{code}</b>, session code is <b>{sessionCode}</b>.", true);
        }
    }
}
