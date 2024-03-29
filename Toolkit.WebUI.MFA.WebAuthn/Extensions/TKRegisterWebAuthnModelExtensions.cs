using Fido2NetLib;
using Fido2NetLib.Objects;
using GeoCoordinatePortable;
using QoDL.Toolkit.WebUI.Models;
using static QoDL.Toolkit.WebUI.Models.TKVerifyWebAuthnAssertionModel;

namespace QoDL.Toolkit.WebUI.MFA.WebAuthn.Extensions;

/// <summary>
/// Extensions for <see cref="TKRegisterWebAuthnModel"/>.
/// </summary>
public static class TKRegisterWebAuthnModelExtensions
{
    /// <summary>
    /// Convert the given <see cref="TKRegisterWebAuthnModel"/> into a <see cref="AuthenticatorAttestationRawResponse"/>.
    /// </summary>
    public static AuthenticatorAttestationRawResponse ToAuthenticatorAttestationRawResponse(this TKRegisterWebAuthnModel model)
    {
        if (model == null) return null;

        return new AuthenticatorAttestationRawResponse
        {
            Id = Base64Url.Decode(model.Id),
            RawId = Base64Url.Decode(model.RawId),
            Type = PublicKeyCredentialType.PublicKey,
            Extensions = model.Extensions.ToAuthenticationExtensionsClientOutputs(),
            Response = new AuthenticatorAttestationRawResponse.ResponseData
            {
                AttestationObject = Base64Url.Decode(model.Response.AttestationObject),
                ClientDataJson = Base64Url.Decode(model.Response.ClientDataJson)
            }
        };
    }

    private static AuthenticationExtensionsClientOutputs ToAuthenticationExtensionsClientOutputs(this TKAuthenticationExtensionsClientOutputs model)
    {
        if (model == null) return null;

        return new AuthenticationExtensionsClientOutputs
        {
            Example = model.Example,
            AppID = model.AppID,
            SimpleTransactionAuthorization = model.txAuthSimple,
            GenericTransactionAuthorization = model.txAuthGenericArg,
            AuthenticatorSelection = model.authnSel,
            Extensions = model.exts,
            UserVerificationIndex = model.uvi,
            Location = model.loc.ToGeoCoordinate(),
            UserVerificationMethod = model.uvm,
            BiometricAuthenticatorPerformanceBounds = model.biometricPerfBounds
        };
    }

    private static GeoCoordinate ToGeoCoordinate(this TKAuthenticationGeoCoordinate model)
    {
        if (model == null) return null;

        return new GeoCoordinate
        {
            Course = model.Course,
            Speed = model.Speed,
            VerticalAccuracy = model.VerticalAccuracy,
            HorizontalAccuracy = model.HorizontalAccuracy,
            Longitude = model.Longitude,
            Latitude = model.Latitude,
            Altitude = model.Altitude
        };
    }
}
