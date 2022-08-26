using System.Collections.Generic;

namespace HealthCheck.Core.Modules.ContentPermutation.Models
{
    /// <summary></summary>
    public class HCPermutatedContentResultViewModel
    {
        /// <summary></summary>
        public bool WasCached { get; set; }

        /// <summary></summary>
        public List<HCPermutatedContentItemViewModel> Content { get; set; }
    }
}
