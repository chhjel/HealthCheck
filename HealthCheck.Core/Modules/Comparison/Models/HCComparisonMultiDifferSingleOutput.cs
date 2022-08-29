using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Comparison.Models
{
    /// <summary></summary>
    public class HCComparisonMultiDifferSingleOutput
    {
        /// <summary></summary>
        public string DifferId { get; set; }

        /// <summary></summary>
        public List<HCComparisonDifferOutputData> Data { get; set; }
    }

}
