using HealthCheck.Core.Modules.Comparison.Models;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Comparison.Abstractions
{
    /// <summary>
    /// Handles comparing two instances.
    /// </summary>
    public abstract class HCComparisonDifferBase<TContent> : IHCComparisonDiffer
        where TContent : class
    {
        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public virtual string Description { get; }

        /// <inheritdoc />
        public virtual int UIOrder => 0;

        /// <inheritdoc />
        public virtual bool CanHandle(IHCComparisonTypeHandler handler) => handler?.ContentType == typeof(TContent);

        /// <inheritdoc />
        public virtual bool DefaultEnabledFor(IHCComparisonTypeHandler handler) => true;

        /// <inheritdoc />
        public virtual Task<HCComparisonDifferOutput> CompareInstancesAsync(object left, object right, string leftName, string rightName)
            => CompareInstancesAsync(left as TContent, right as TContent, leftName, rightName);

        /// <summary>
        /// Compare the given instances and create some output.
        /// </summary>
        public abstract Task<HCComparisonDifferOutput> CompareInstancesAsync(TContent left, TContent right, string leftName, string rightName);
    }
}
