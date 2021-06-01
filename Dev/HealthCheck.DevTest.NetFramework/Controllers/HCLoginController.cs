using HealthCheck.WebUI.Abstractions;
using HealthCheck.WebUI.MFA.TOTP;
using HealthCheck.WebUI.Models;
using System;

namespace HealthCheck.DevTest.Controllers
{
    public class HCLoginController : HealthCheckLoginControllerBase
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

            var otpOk = HCMfaTotpUtil.ValidateTotpCode(DummySecret, request.TwoFactorCode);
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

            var code = HCMfaTotpUtil.GenerateTotpCode(DummySecret);
            var sessionCode = CreateSession2FACode(request.Username);
            return HCIntegratedLogin2FACodeRequestResult.CreateSuccess($"TOTP code is <b>{code}</b>, session code is <b>{sessionCode}</b>.",
                showAsHtml: true,
                codeExpiresIn: TimeSpan.FromMinutes(5));
        }
    }
}
