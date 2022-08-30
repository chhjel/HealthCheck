using HealthCheck.Core.Modules.Comparison.Models;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Comparison.Abstractions
{
    /// <summary>
    /// Handles comparing two instances.
    /// </summary>
    public interface IHCComparisonDiffer
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
        bool CanHandle(IHCComparisonTypeHandler handler);

        /// <summary>
        /// Return true if the differ should be enabled by default for the given handler.
        /// </summary>
        bool DefaultEnabledFor(IHCComparisonTypeHandler handler);

        /// <summary>
        /// Compare the given instances and create some output.
        /// </summary>
        Task<HCComparisonDifferOutput> CompareInstancesAsync(object left, object right, string leftName, string rightName);
    }
}
