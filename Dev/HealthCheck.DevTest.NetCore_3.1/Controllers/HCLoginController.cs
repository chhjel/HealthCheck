using Fido2NetLib;
using Fido2NetLib.Objects;
using HealthCheck.Core.Attributes;
using HealthCheck.WebUI.Abstractions;
using HealthCheck.WebUI.MFA.FIDO2;
using HealthCheck.WebUI.MFA.TOTP;
using HealthCheck.WebUI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.DevTest.NetCore_3._1.Controllers
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

            var otpOk = string.IsNullOrWhiteSpace(request.TwoFactorCode) || HCMfaTotpUtil.ValidateTotpCode(DummySecret, request.TwoFactorCode);
            if (!otpOk)
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
            return HCIntegratedLogin2FACodeRequestResult.CreateSuccess($"TOTP code is <b>{code}</b>", true);
        }

        protected virtual HCFido2Helper CreateFido()
            => new HCFido2Helper("localhost", "HCDev", Request.Headers["Origin"]);

        [HideFromRequestLog]
        [HttpPost]
        [Route("CreateFidoRegistrationOptions")]
        public virtual async Task<ActionResult> CreateFidoRegistrationOptions([FromBody] CreateFidoRegistrationOptionsRequest request)
        {
            if (!Enabled) return NotFound();
            await Delay();

            if (!ModelState.IsValid)
            {
                var errors = string.Join("\n", ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage));
                return BadRequest(errors);
            }

            var fido = CreateFido();
            var options = fido.CreateClientOptions(request.Username);
            HttpContext.Session.SetString("fido2.attestationOptions", options.ToJson());
            return CreateJsonResult(options, stringEnums: false);
        }

        public class CreateFidoRegistrationOptionsRequest
        {
            public string Username { get; set; }
        }

        [HideFromRequestLog]
        [HttpPost]
        [Route("RegisterFido")]
        public virtual async Task<ActionResult> RegisterFido([FromBody] RegisterFidoRequest request)
        {
            if (!Enabled) return NotFound();
            await Delay();

            if (!ModelState.IsValid)
            {
                var errors = string.Join("\n", ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage));
                return BadRequest(errors);
            }

            var fido = CreateFido();
            var jsonOptions = HttpContext.Session.GetString("fido2.attestationOptions");
            var options = CredentialCreateOptions.FromJson(jsonOptions);

            var convertedRequest = request.ToAuthenticatorAttestationRawResponse();
            await fido.RegisterCredentials(options, convertedRequest);
            return CreateJsonResult("OK");
        }

        public class RegisterFidoRequest
        {
            //[JsonConverter(typeof(Base64UrlConverter))]
            public string Id { get; set; }
            //[JsonConverter(typeof(Base64UrlConverter))]
            public string RawId { get; set; }
            //public PublicKeyCredentialType? Type { get; set; }
            public ResponseData Response { get; set; }
            public AuthenticationExtensionsClientOutputs Extensions { get; set; }

            public class ResponseData
            {
                //[JsonConverter(typeof(Base64UrlConverter))]
                public string AttestationObject { get; set; }
                //[JsonConverter(typeof(Base64UrlConverter))]
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

        [HideFromRequestLog]
        [HttpPost]
        [Route("CreateFidoAssertionOptions")]
        public virtual async Task<ActionResult> CreateFidoAssertionOptions([FromBody] CreateFidoAssertionOptionsRequest request)
        {
            if (!Enabled) return NotFound();
            await Delay();

            if (!ModelState.IsValid)
            {
                var errors = string.Join("\n", ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage));
                return BadRequest(errors);
            }

            var fido = CreateFido();
            var options = fido.CreateAssertionOptions(request.Username);
            return CreateJsonResult(options);
        }

        public class CreateFidoAssertionOptionsRequest
        {
            public string Username { get; set; }
        }

        [HideFromRequestLog]
        [HttpPost]
        [Route("VerifyFido")]
        public virtual async Task<ActionResult> VerifyFido([FromBody] VerifyFidoRequest request)
        {
            if (!Enabled) return NotFound();
            await Delay();

            if (!ModelState.IsValid)
            {
                var errors = string.Join("\n", ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage));
                return BadRequest(errors);
            }

            var fido = CreateFido();
            var result = fido.VerifyAssertion(request.Options, request.AttestationResponse);
            return CreateJsonResult(result);
        }

        public class VerifyFidoRequest
        {
            public AssertionOptions Options { get; set; }
            public AuthenticatorAssertionRawResponse AttestationResponse { get; set; }
        }
    }
}
