using System;

namespace QoDL.Toolkit.Module.IPWhitelist.Models;

/// <summary></summary>
public class TKIPWhitelistIP
{
    /// <summary></summary>
    public Guid Id { get; set; }

    /// <summary></summary>
    public Guid RuleId { get; set; }

    /// <summary></summary>
    public string IP { get; set; }

    /// <summary></summary>
    public string Note { get; set; }
}
