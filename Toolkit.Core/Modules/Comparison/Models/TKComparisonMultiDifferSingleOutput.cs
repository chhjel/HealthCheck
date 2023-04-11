using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Comparison.Models
{
    /// <summary></summary>
    public class TKComparisonMultiDifferSingleOutput
    {
        /// <summary></summary>
        public string DifferId { get; set; }

        /// <summary></summary>
        public List<TKComparisonDifferOutputData> Data { get; set; }
    }

}
