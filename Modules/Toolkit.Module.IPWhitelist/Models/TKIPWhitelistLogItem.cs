using System;

namespace QoDL.Toolkit.Module.IPWhitelist.Models;

/// <summary></summary>
public class TKIPWhitelistLogItem
{
    /// <summary></summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary></summary>
    public string IP { get; set; }

    /// <summary></summary>
    public string Method { get; set; }

    /// <summary></summary>
    public string Path { get; set; }

    /// <summary></summary>
    public bool WasBlocked { get; set; }

    /// <summary></summary>
    public string Note { get; set; }
}
