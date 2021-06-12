using Fido2NetLib;
using HealthCheck.Core.Util;
using HealthCheck.WebUI.Abstractions;
using HealthCheck.WebUI.MFA.TOTP;
using HealthCheck.WebUI.MFA.WebAuthn;
using HealthCheck.WebUI.MFA.WebAuthn.Storage;
using HealthCheck.WebUI.Models;
using System.Linq;

namespace HealthCheck.DevTest.NetCore_3._1.Controllers
{
    public class HCLoginController : HealthCheckLoginControllerBase
    {
        private const string DummyTotpSecret = "J5V5XFSQCT2TDG6AZIQ46TTEAXGU7GCW";

        protected override HCIntegratedLoginResult HandleLoginRequest(HCIntegratedLoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("\n", ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage));
                return HCIntegratedLoginResult.CreateError(errors);
            }

            var userPassOk = request.Username == "root"
                && request.Password == "toor";

            if (!userPassOk)
            {
                return HCIntegratedLoginResult.CreateError("Wrong username or password, try again or <b>give up</b>.<br /><a href=\"https://www.google.com\">Help me!</a>", true);
            }

            var otpOk = string.IsNullOrWhiteSpace(request.TwoFactorCode) || HCMfaTotpUtil.ValidateTotpCode(DummyTotpSecret, request.TwoFactorCode);
            if (!otpOk)
            {
                return HCIntegratedLoginResult.CreateError("Two-factor code was wrong, try again.");
            }

            if (request.WebAuthnPayload?.Id != null)
            {
                var webauthn = CreateWebAuthnHelper();
                var jsonOptions = GetWebAuthnAssertionOptionsJsonForSession();
                var options = AssertionOptions.FromJson(jsonOptions);
                var webAuthnResult = AsyncUtils.RunSync(() => webauthn.VerifyAssertion(options, request.WebAuthnPayload));
                if (!webAuthnResult.Success)
                {
                    return HCIntegratedLoginResult.CreateError(webAuthnResult.Error);
                }
            }

            return HCIntegratedLoginResult.CreateSuccess();
        }

        protected override HCIntegratedLogin2FACodeRequestResult Handle2FACodeRequest(HCIntegratedLoginRequest2FACodeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                return HCIntegratedLogin2FACodeRequestResult.CreateError("You must enter your username first.");
            }

            var code = HCMfaTotpUtil.GenerateTotpCode(DummyTotpSecret);
            return HCIntegratedLogin2FACodeRequestResult.CreateSuccess($"TOTP code is <b>{code}</b>", true);
        }

        protected override string CreateWebAuthnAssertionOptionsJson(HCIntegratedLoginCreateWebAuthnAssertionOptionsRequest request)
        {
            var webauthn = CreateWebAuthnHelper();
            var options = webauthn.CreateAssertionOptions(request.Username);
            return options?.ToJson();
        }

        private HCWebAuthnHelper CreateWebAuthnHelper()
            => new HCWebAuthnHelper(new HCWebAuthnHelperOptions
            {
                ServerDomain = "localhost",
                ServerName = "HCDev",
                Origin = Request.Headers["Origin"]
            }, new HCMemoryWebAuthnCredentialManager());
    }
}
