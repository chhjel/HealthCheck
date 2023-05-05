using QoDL.Toolkit.Module.IPWhitelist.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.IPWhitelist.Abstractions;

/// <summary></summary>
public interface ITKIPWhitelistIPStorage
{
    /// <summary></summary>
    Task<IEnumerable<TKIPWhitelistIP>> GetAllIPsAsync();

    /// <summary></summary>
    Task<IEnumerable<TKIPWhitelistIP>> GetRuleIPsAsync(Guid ruleId);

    /// <summary></summary>
    Task DeleteRuleIPAsync(Guid id);

    /// <summary></summary>
    Task DeleteRuleIPsAsync(Guid ruleId);

    /// <summary></summary>
    Task<TKIPWhitelistIP> StoreRuleIPAsync(TKIPWhitelistIP ip);
}
