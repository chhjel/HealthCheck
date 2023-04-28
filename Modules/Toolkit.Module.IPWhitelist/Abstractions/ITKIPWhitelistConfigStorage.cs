using QoDL.Toolkit.Module.IPWhitelist.Models;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.IPWhitelist.Abstractions;

/// <summary></summary>
public interface ITKIPWhitelistConfigStorage
{
    /// <summary></summary>
    Task<TKIPWhitelistConfig> GetConfigAsync();

    /// <summary></summary>
    Task SaveConfigAsync(TKIPWhitelistConfig config);
}
