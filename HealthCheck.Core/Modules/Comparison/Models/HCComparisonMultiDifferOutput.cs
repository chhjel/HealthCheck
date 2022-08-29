using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Comparison.Models
{
    /// <summary></summary>
    public class HCComparisonMultiDifferOutput
    {
        /// <summary></summary>
        public List<HCComparisonMultiDifferSingleOutput> Data { get; set; } = new();
    }
}
