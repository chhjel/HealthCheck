using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Modules.Comparison.Abstractions;
using QoDL.Toolkit.Core.Modules.Comparison.Models;
using System;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.Comparison
{
    /// <summary>
    /// Diffs by serializing instances to json and showing a diff output.
    /// </summary>
    public class TKComparisonDifferSerializedJson : ITKComparisonDiffer
    {
        /// <inheritdoc />
        public virtual string Name => "Raw Json diff";

        /// <inheritdoc />
        public virtual string Description => "Compare all data as Json.";

        /// <inheritdoc />
        public int UIOrder => -10;

        /// <summary>
        /// When instances are equal, include a note mentioning it.
        /// <para>Defaults to true.</para>
        /// </summary>
        public bool AllowEqualNote { get; set; } = true;

        /// <inheritdoc />
        public virtual bool CanHandle(ITKComparisonTypeHandler handler) => true;

        /// <inheritdoc />
        public virtual bool DefaultEnabledFor(ITKComparisonTypeHandler handler) => true;

        /// <inheritdoc />
        public virtual Task<TKComparisonDifferOutput> CompareInstancesAsync(object left, object right, string leftName, string rightName)
        {
            var leftJson = TKGlobalConfig.Serializer.Serialize(left, true);
            var rightJson = TKGlobalConfig.Serializer.Serialize(right, true);

            var result = new TKComparisonDifferOutput();
            if (AllowEqualNote && leftJson.Equals(rightJson, StringComparison.InvariantCulture))
            {
                result.AddNote($"{leftName ?? "left side"} is equal to {rightName ?? "right side"}", string.Empty);
            }
            result.AddDiff(leftJson, rightJson, Name, leftName, rightName);

            return Task.FromResult(result);
        }
    }
}
