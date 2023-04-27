using QoDL.Toolkit.Module.IPWhitelist.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.IPWhitelist.Abstractions;

/// <summary></summary>
public interface ITKIPWhitelistRuleStorage
{
    /// <summary></summary>
    public Task<IEnumerable<TKIPWhitelistRule>> GetRulesAsync();
}
