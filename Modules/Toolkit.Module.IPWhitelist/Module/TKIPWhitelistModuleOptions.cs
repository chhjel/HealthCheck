using QoDL.Toolkit.Module.IPWhitelist.Abstractions;

namespace QoDL.Toolkit.Module.IPWhitelist.Module;

/// <summary>
/// Options for <see cref="TKIPWhitelistModule"/>.
/// </summary>
public class TKIPWhitelistModuleOptions
{
    /// <summary>
    /// 
    /// </summary>
    public ITKIPWhitelistService Service { get; set; }
}
