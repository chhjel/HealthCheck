using QoDL.Toolkit.Core.Modules.Comparison.Models;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.Comparison.Abstractions
{
    /// <summary>
    /// Handles comparing two instances.
    /// </summary>
    public abstract class TKComparisonDifferBase<TContent> : ITKComparisonDiffer
        where TContent : class
    {
        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public virtual string Description { get; }

        /// <inheritdoc />
        public virtual int UIOrder => 0;

        /// <inheritdoc />
        public virtual bool CanHandle(ITKComparisonTypeHandler handler) => handler?.ContentType == typeof(TContent);

        /// <inheritdoc />
        public virtual bool DefaultEnabledFor(ITKComparisonTypeHandler handler) => true;

        /// <inheritdoc />
        public virtual Task<TKComparisonDifferOutput> CompareInstancesAsync(object left, object right, string leftName, string rightName)
            => CompareInstancesAsync(left as TContent, right as TContent, leftName, rightName);

        /// <summary>
        /// Compare the given instances and create some output.
        /// </summary>
        public abstract Task<TKComparisonDifferOutput> CompareInstancesAsync(TContent left, TContent right, string leftName, string rightName);
    }
}
