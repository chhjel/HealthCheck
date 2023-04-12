using QoDL.Toolkit.Core.Util.Models;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Settings.Models;

/// <summary>
/// Model used when retrieving settings.
/// </summary>
public class GetSettingsViewModel
{
    /// <summary>
    /// Setting definitions.
    /// </summary>
    public List<TKBackendInputConfig> Definitions { get; set; } = new List<TKBackendInputConfig>();

    /// <summary>
    /// Setting values.
    /// </summary>
    public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();
}
