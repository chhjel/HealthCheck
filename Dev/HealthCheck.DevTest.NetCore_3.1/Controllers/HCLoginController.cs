using Fido2NetLib;
using Fido2NetLib.Objects;
using HealthCheck.Core.Attributes;
using HealthCheck.Core.Util;
using HealthCheck.WebUI.Abstractions;
using HealthCheck.WebUI.MFA.TOTP;
using HealthCheck.WebUI.MFA.WebAuthn;
using HealthCheck.WebUI.MFA.WebAuthn.Storage;
using HealthCheck.WebUI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

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

            if (request.WebAuthnPayload != null)
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

        private HCWebAuthnHelper CreateWebAuthnHelper() => new HCWebAuthnHelper("localhost", "HCDev", Request.Headers["Origin"], new HCMemoryWebAuthnCredentialManager());

        [HideFromRequestLog]
        [HttpPost]
        [Route("CreateWebAuthnRegistrationOptions")]
        public virtual ActionResult CreateWebAuthnRegistrationOptions([FromBody] CreateWebAuthnRegistrationOptionsRequest request)
        {
            if (!Enabled) return NotFound();

            if (!ModelState.IsValid)
            {
                var errors = string.Join("\n", ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage));
                return BadRequest(errors);
            }

            var webauthn = CreateWebAuthnHelper();
            var options = webauthn.CreateClientOptions(request.Username);
            HttpContext.Session.SetString("WebAuthn.attestationOptions", options.ToJson());
            return CreateJsonResult(options, stringEnums: false);
        }
        public class CreateWebAuthnRegistrationOptionsRequest
        {
            public string Username { get; set; }
        }

        [HideFromRequestLog]
        [HttpPost]
        [Route("RegisterWebAuthn")]
        public virtual async Task<ActionResult> RegisterWebAuthn([FromBody] RegisterWebAuthnRequest request)
        {
            if (!Enabled) return NotFound();

            if (!ModelState.IsValid)
            {
                var errors = string.Join("\n", ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage));
                return BadRequest(errors);
            }

            var webauthn = CreateWebAuthnHelper();
            var jsonOptions = HttpContext.Session.GetString("WebAuthn.attestationOptions");
            var options = CredentialCreateOptions.FromJson(jsonOptions);

            var convertedRequest = request.ToAuthenticatorAttestationRawResponse();
            await webauthn.RegisterCredentials(options, convertedRequest);
            return CreateJsonResult("OK");
        }
        public class RegisterWebAuthnRequest
        {
            public string Id { get; set; }
            public string RawId { get; set; }
            public ResponseData Response { get; set; }
            public AuthenticationExtensionsClientOutputs Extensions { get; set; }

            public class ResponseData
            {
                public string AttestationObject { get; set; }
                public string ClientDataJson { get; set; }
            }

            public AuthenticatorAttestationRawResponse ToAuthenticatorAttestationRawResponse()
            {
                return new AuthenticatorAttestationRawResponse
                {
                    Id = Base64Url.Decode(Id),
                    RawId = Base64Url.Decode(RawId),
                    Type = PublicKeyCredentialType.PublicKey,
                    Extensions = Extensions,
                    Response = new AuthenticatorAttestationRawResponse.ResponseData
                    {
                        AttestationObject = Base64Url.Decode(Response.AttestationObject),
                        ClientDataJson = Base64Url.Decode(Response.ClientDataJson)
                    }
                };
            }
        }
    }
}
