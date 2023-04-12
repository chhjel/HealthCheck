using System;

namespace QoDL.Toolkit.WebUI.MFA.WebAuthn;

/// <summary>
/// Options for <see cref="TKWebAuthnHelper"/>.
/// </summary>
public class TKWebAuthnHelperOptions
{
    /// <summary>
    /// Domain of the server the site is running on. For localhost use 'localhost'.
    /// </summary>
    public string ServerDomain { get; set; }

    /// <summary>
    /// Display name shown in the browser popup when requesting access to the FIDO key.
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    /// Origin from request headers. E.g. <c>Request.Headers["Origin"]</c>
    /// </summary>
    public string Origin { get; set; }

    /// <summary>
    /// Time that will be allowed for clock drift on a timestamped attestation.
    /// <para>Defaults to 5 minutes.</para>
    /// </summary>
    public TimeSpan TimestampDriftTolerance { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Throws <see cref="ArgumentException" /> if any required config values are missing.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ServerDomain)) throw new ArgumentException($"{nameof(ServerDomain)} must be set.");
        else if (string.IsNullOrWhiteSpace(ServerName)) throw new ArgumentException($"{nameof(ServerName)} must be set.");
        else if (string.IsNullOrWhiteSpace(Origin)) throw new ArgumentException($"{nameof(Origin)} must be set.");
    }
}
