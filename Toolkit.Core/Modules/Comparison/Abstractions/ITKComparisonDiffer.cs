using QoDL.Toolkit.Core.Modules.Comparison.Models;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.Comparison.Abstractions;

/// <summary>
/// Handles comparing two instances.
/// </summary>
public interface ITKComparisonDiffer
{
    /// <summary>
    /// Name of the differ.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Optional description of the differ.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Order in the UI, both for the selection and output.
    /// <para>Lower value = lower on page.</para>
    /// </summary>
    int UIOrder { get; }

    /// <summary>
    /// Return true if the differ supports the given type.
    /// </summary>
    bool CanHandle(ITKComparisonTypeHandler handler);

    /// <summary>
    /// Return true if the differ should be enabled by default for the given handler.
    /// </summary>
    bool DefaultEnabledFor(ITKComparisonTypeHandler handler);

    /// <summary>
    /// Compare the given instances and create some output.
    /// </summary>
    Task<TKComparisonDifferOutput> CompareInstancesAsync(object left, object right, string leftName, string rightName);
}
