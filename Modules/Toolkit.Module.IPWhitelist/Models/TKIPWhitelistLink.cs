using System;

namespace QoDL.Toolkit.Module.IPWhitelist.Models;

/// <summary></summary>
public class TKIPWhitelistLink
{
    /// <summary></summary>
    public Guid Id { get; set; }

    /// <summary></summary>
    public Guid RuleId { get; set; }

    /// <summary></summary>
    public string Secret { get; set; }

    /// <summary></summary>
    public DateTimeOffset? InvitationExpiresAt { get; set; }

    /// <summary></summary>
    public string Name { get; set; }

    /// <summary></summary>
    public string Note { get; set; }
}
