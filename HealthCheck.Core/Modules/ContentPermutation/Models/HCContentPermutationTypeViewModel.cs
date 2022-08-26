using HealthCheck.Core.Util.Models;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.ContentPermutation.Models
{
    /// <summary></summary>
    public class HCContentPermutationTypeViewModel
    {
        /// <summary></summary>
        public string Id { get; set; }

        /// <summary></summary>
        public string Name { get; set; }

        /// <summary></summary>
        public string Description { get; set; }

        /// <summary></summary>
        public List<HCContentPermutationChoiceViewModel> Permutations { get; set; }

        /// <summary>
        /// Property details.
        /// </summary>
        public List<HCBackendInputConfig> PropertyConfigs { get; set; }
    }
}
