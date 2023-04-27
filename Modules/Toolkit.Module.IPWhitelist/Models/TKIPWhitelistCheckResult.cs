using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Module.IPWhitelist.Models;

/// <summary></summary>
public class TKIPWhitelistCheckResult
{
    /// <summary></summary>
    public bool Blocked { get; set; }

    /// <summary></summary>
    public string Response { get; set; }
    /// <summary></summary>
    public int? HttpStatusCode { get; set; }
}
