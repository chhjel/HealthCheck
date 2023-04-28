using System;

namespace QoDL.Toolkit.Module.IPWhitelist.Models;

/// <summary></summary>
public class TKIPWhitelistConfig
{
    /// <summary></summary>
    public string DefaultResponse { get; set; }

    /// <summary></summary>
    public int? DefaultHttpStatusCode { get; set; }
}
