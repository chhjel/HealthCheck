using Fido2NetLib;
using Fido2NetLib.Objects;
using HealthCheck.WebUI.Models;

namespace HealthCheck.WebUI.MFA.WebAuthn.Extensions
{
    /// <summary>
    /// Extensions for <see cref="VerifyWebAuthnAssertionModel"/>.
    /// </summary>
    public static class VerifyFidoAssertionRequestExtensions
    {
        /// <summary>
        /// Convert into a <see cref="AuthenticatorAssertionRawResponse"/>.
        /// </summary>
        public static AuthenticatorAssertionRawResponse ToAuthenticatorAssertionRawResponse(this VerifyWebAuthnAssertionModel model)
        {
            if (model == null)
            {
                return new AuthenticatorAssertionRawResponse();
            }

            return new AuthenticatorAssertionRawResponse
            {
                Id = Base64Url.Decode(model.Id),
                RawId = Base64Url.Decode(model.RawId),
                Type = PublicKeyCredentialType.PublicKey,
                Extensions = new AuthenticationExtensionsClientOutputs
                {
                    Example = model.Extensions?.Example ?? default,
                    AppID = model.Extensions?.AppID ?? default,
                    SimpleTransactionAuthorization = model.Extensions?.txAuthSimple ?? default,
                    GenericTransactionAuthorization = model.Extensions?.txAuthGenericArg ?? default,
                    AuthenticatorSelection = model.Extensions?.authnSel ?? default,
                    Extensions = model.Extensions?.exts ?? default,
                    UserVerificationIndex = model.Extensions?.uvi ?? default,
                    Location = new GeoCoordinatePortable.GeoCoordinate
                    {
                        Altitude = model.Extensions?.loc?.Altitude ?? default,

                    },
                    UserVerificationMethod = model.Extensions?.uvm ?? default,
                    BiometricAuthenticatorPerformanceBounds = model.Extensions?.biometricPerfBounds ?? default
                },
                Response = new AuthenticatorAssertionRawResponse.AssertionResponse
                {
                    AuthenticatorData = Base64Url.Decode(model.Response.AuthenticatorData),
                    Signature = Base64Url.Decode(model.Response.Signature),
                    ClientDataJson = Base64Url.Decode(model.Response.ClientDataJson),
                    //UserHandle = Base64Url.Decode(model.Response.UserHandle)
                }
            };
        }
    }
}
