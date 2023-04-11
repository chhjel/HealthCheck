using Fido2NetLib;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.WebUI.Abstractions;
using QoDL.Toolkit.WebUI.MFA.TOTP;
using QoDL.Toolkit.WebUI.MFA.WebAuthn;
using QoDL.Toolkit.WebUI.MFA.WebAuthn.Storage;
using QoDL.Toolkit.WebUI.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;

namespace QoDL.Toolkit.DevTest.NetCore_6._0.Controllers.LoginTestControllers
{
    public class TKLoginController : ToolkitLoginControllerBase
    {
        private const string DummyTotpSecret = "J5V5XFSQCT2TDG6AZIQ46TTEAXGU7GCW";

        protected override TKIntegratedLoginResult HandleLoginRequest(TKIntegratedLoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("\n", ModelState.SelectMany(x => x.Value?.Errors ?? Enumerable.Empty<ModelError>()).Select(x => x.ErrorMessage));
                return TKIntegratedLoginResult.CreateError(errors);
            }

            var userPassOk = request.Username == "root"
                && request.Password == "toor";

            if (!userPassOk)
            {
                return TKIntegratedLoginResult.CreateError("Wrong username or password, try again or <b>give up</b>.<br /><a href=\"https://www.google.com\">Help me!</a>", true);
            }

            var otpOk = string.IsNullOrWhiteSpace(request.TwoFactorCode) || TKMfaTotpUtil.ValidateTotpCode(DummyTotpSecret, request.TwoFactorCode);
            if (!otpOk)
            {
                return TKIntegratedLoginResult.CreateError("Two-factor code was wrong, try again.");
            }

            if (request.WebAuthnPayload?.Id != null)
            {
                var webauthn = CreateWebAuthnHelper();
                var jsonOptions = GetWebAuthnAssertionOptionsJsonForSession();
                var options = AssertionOptions.FromJson(jsonOptions);
                var webAuthnResult = TKAsyncUtils.RunSync(() => webauthn.VerifyAssertion(options, request.WebAuthnPayload));
                if (!webAuthnResult.Success)
                {
                    return TKIntegratedLoginResult.CreateError(webAuthnResult.Error);
                }
            }

            return TKIntegratedLoginResult.CreateSuccess();
        }

        protected override TKIntegratedLogin2FACodeRequestResult Handle2FACodeRequest(TKIntegratedLoginRequest2FACodeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                return TKIntegratedLogin2FACodeRequestResult.CreateError("You must enter your username first.");
            }

            var code = TKMfaTotpUtil.GenerateTotpCode(DummyTotpSecret);
            return TKIntegratedLogin2FACodeRequestResult.CreateSuccess($"TOTP code is <b>{code}</b>", true);
        }

        protected override string? CreateWebAuthnAssertionOptionsJson(TKIntegratedLoginCreateWebAuthnAssertionOptionsRequest request)
        {
            var webauthn = CreateWebAuthnHelper();
            var options = webauthn.CreateAssertionOptions(request.Username);
            return options?.ToJson();
        }

        private TKWebAuthnHelper CreateWebAuthnHelper()
            => new(new TKWebAuthnHelperOptions
            {
                ServerDomain = "localhost",
                ServerName = "TKDev",
                Origin = Request.Headers["Origin"]
            }, new TKMemoryWebAuthnCredentialManager());
    }
}
