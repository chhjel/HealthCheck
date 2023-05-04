namespace QoDL.Toolkit.Module.IPWhitelist.Models;

/// <summary></summary>
public class TKIPWhitelistConfig
{
    /// <summary></summary>
    public string DefaultResponse { get; set; } = "⛔ 403 ⛔";

    /// <summary></summary>
    public bool UseDefaultResponseWrapper { get; set; } = true;

    /// <summary></summary>
    public int? DefaultHttpStatusCode { get; set; } = 403;
}
