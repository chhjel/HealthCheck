namespace QoDL.Toolkit.Module.IPWhitelist.Models;

/// <summary></summary>
public class TKIPWhitelistCheckResult
{
    /// <summary></summary>
    public static TKIPWhitelistCheckResult CreateAllowed(string reason, TKIPWhitelistRule allowingRule = null)
        => new()
        {
            AllowedReason = reason,
            AllowingRule = allowingRule
        };

    /// <summary></summary>
    public static TKIPWhitelistCheckResult CreateBlocked(string response, int? httpStatusCode, bool useDefaultResponseWrapper)
        => new()
        {
            Blocked = true,
            Response = response,
            UseDefaultResponseWrapper = useDefaultResponseWrapper,
            HttpStatusCode = httpStatusCode
        };

    /// <summary></summary>
    public bool Blocked { get; set; }

    /// <summary></summary>
    public TKIPWhitelistRule AllowingRule { get; set; }

    /// <summary></summary>
    public string AllowedReason { get; set; }

    /// <summary></summary>
    public string Response { get; set; }

    /// <summary></summary>
    public bool UseDefaultResponseWrapper { get; set; }

    /// <summary></summary>
    public int? HttpStatusCode { get; set; }
}
