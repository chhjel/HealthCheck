using QoDL.Toolkit.Module.IPWhitelist.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.IPWhitelist.Abstractions;

/// <summary></summary>
public interface ITKIPWhitelistRuleStorage
{
    /// <summary></summary>
    Task<IEnumerable<TKIPWhitelistRule>> GetRulesAsync();

    /// <summary></summary>
    Task DeleteRuleAsync(Guid id);

    /// <summary></summary>
    Task<TKIPWhitelistRule> StoreRuleAsync(TKIPWhitelistRule rule);
}
