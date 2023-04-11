using QoDL.Toolkit.Core.Modules.Comparison.Abstractions;

namespace QoDL.Toolkit.Core.Modules.Comparison;

/// <summary>
/// Options for <see cref="TKComparisonModule"/>.
/// </summary>
public class TKComparisonModuleOptions
{
    /// <summary>
    /// Service that handles the comparing.
    /// </summary>
    public ITKComparisonService Service { get; set; }
}
