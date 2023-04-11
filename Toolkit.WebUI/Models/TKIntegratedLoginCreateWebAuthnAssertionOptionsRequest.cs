namespace QoDL.Toolkit.WebUI.Models;

/// <summary>
/// Payload sent to CreateWebAuthnAssertionOptions.
/// </summary>
public class TKIntegratedLoginCreateWebAuthnAssertionOptionsRequest
{
    /// <summary>
    /// Username the user attempts to login with.
    /// </summary>
    public string Username { get; set; }
}
