using QoDL.Toolkit.WebUI.Abstractions;
using QoDL.Toolkit.WebUI.MFA.TOTP;
using QoDL.Toolkit.WebUI.Models;
using System;

namespace QoDL.Toolkit.DevTest.Controllers
{
    public class TKLoginController : ToolkitLoginControllerBase
    {
        public const string DummySecret = "J5V5XFSQCT2TDG6AZIQ46TTEAXGU7GCW";

        protected override TKIntegratedLoginResult HandleLoginRequest(TKIntegratedLoginRequest request)
        {
            var userPassOk = request.Username == "root"
                && request.Password == "toor";

            if (!userPassOk)
            {
                return TKIntegratedLoginResult.CreateError("Wrong username or password, try again or <b>give up</b>.<br /><a href=\"https://www.google.com\">Help me!</a>", true);
            }

            var otpOk = TKMfaTotpUtil.ValidateTotpCode(DummySecret, request.TwoFactorCode);
            var sessionCodeValid = ValidateSession2FACode(request.Username, request.TwoFactorCode);

            if (!otpOk && !sessionCodeValid)
            {
                return TKIntegratedLoginResult.CreateError("Two-factor code was wrong, try again.");
            }

            return TKIntegratedLoginResult.CreateSuccess();
        }

        protected override TKIntegratedLogin2FACodeRequestResult Handle2FACodeRequest(TKIntegratedLoginRequest2FACodeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                return TKIntegratedLogin2FACodeRequestResult.CreateError("You must enter your username first.");
            }

            var code = TKMfaTotpUtil.GenerateTotpCode(DummySecret);
            var sessionCode = CreateSession2FACode(request.Username);
            return TKIntegratedLogin2FACodeRequestResult.CreateSuccess($"TOTP code is <b>{code}</b>, session code is <b>{sessionCode}</b>.",
                showAsHtml: true,
                codeExpiresIn: TimeSpan.FromMinutes(5));
        }
    }
}
