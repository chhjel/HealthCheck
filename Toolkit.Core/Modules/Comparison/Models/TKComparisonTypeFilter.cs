using QoDL.Toolkit.Core.Modules.Comparison.Abstractions;

namespace QoDL.Toolkit.Core.Modules.Comparison.Models
{
    /// <summary>
    /// Filter passed to <see cref="ITKComparisonTypeHandler.GetFilteredOptionsAsync"/>
    /// </summary>
    public class TKComparisonTypeFilter
    {
        /// <summary>
        /// User input.
        /// </summary>
        public string Input { get; set; }
    }
}
