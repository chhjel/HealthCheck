namespace QoDL.Toolkit.Module.IPWhitelist.Models;

/// <summary></summary>
public class TKIPWhitelistConfig
{
    /// <summary></summary>
    public bool Enabled { get; set; }

    /// <summary></summary>
    public string DefaultResponseTitle { get; set; } = "403";

    /// <summary></summary>
    public string DefaultResponse { get; set; } = "Access to the requested resource is forbidden. Your IP address has not been whitelisted and therefore you are unable to access this resource.";

    /// <summary></summary>
    public bool UseDefaultResponseWrapper { get; set; } = true;

    /// <summary></summary>
    public int? DefaultHttpStatusCode { get; set; } = 403;
}
