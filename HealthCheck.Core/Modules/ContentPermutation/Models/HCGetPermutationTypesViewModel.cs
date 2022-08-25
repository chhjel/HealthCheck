using System.Collections.Generic;

namespace HealthCheck.Core.Modules.ContentPermutation.Models
{
    /// <summary></summary>
    public class HCGetPermutationTypesViewModel
    {
        /// <summary></summary>
        public List<HCContentPermutationTypeViewModel> Types { get; set; } = new();
    }
}
