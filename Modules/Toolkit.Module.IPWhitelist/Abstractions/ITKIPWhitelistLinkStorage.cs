using QoDL.Toolkit.Module.IPWhitelist.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.IPWhitelist.Abstractions;

/// <summary></summary>
public interface ITKIPWhitelistLinkStorage
{
    /// <summary></summary>
    Task<IEnumerable<TKIPWhitelistLink>> GetRuleLinksAsync(Guid ruleId);

    /// <summary></summary>
    Task DeleteRuleLinkAsync(Guid id);

    /// <summary></summary>
    Task DeleteRuleLinksAsync(Guid ruleId);

    /// <summary></summary>
    Task<TKIPWhitelistLink> StoreRuleLinkAsync(TKIPWhitelistLink link);
}
