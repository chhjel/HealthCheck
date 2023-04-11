using QoDL.Toolkit.Core.Modules.Comparison.Abstractions;

namespace QoDL.Toolkit.Core.Modules.Comparison.Models;

/// <summary>
/// Model sent to frontend from <see cref="ITKComparisonTypeHandler.GetFilteredOptionsAsync"/>
/// </summary>
public class TKComparisonInstanceSelection
{
    /// <summary>
    /// Any id that can be resolved by your own logic in <see cref="ITKComparisonTypeHandler.GetInstanceWithIdAsync"/>.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Display name of this instance.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Optional description of this instance.
    /// </summary>
    public string Description { get; set; }
}
