using System.Collections.Generic;

namespace HealthCheck.Core.Modules.ContentPermutation.Models
{
    /// <summary></summary>
    public class HCGetPermutationTypesViewModel
    {
        /// <summary></summary>
        public List<HCContentPermutationType> Types { get; set; } = new();
    }
}
