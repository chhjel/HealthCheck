using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Module.IPWhitelist.Models;

/// <summary></summary>
public class TKIPWhitelistRule
{
    /// <summary></summary>
    public Guid Id { get; set; }

    /// <summary></summary>
    public bool Enabled { get; set; }

    /// <summary></summary>
    public DateTimeOffset? EnabledUntil { get; set; }

    /// <summary></summary>
    public string Name { get; set; }

    /// <summary></summary>
    public string Note { get; set; }
}
