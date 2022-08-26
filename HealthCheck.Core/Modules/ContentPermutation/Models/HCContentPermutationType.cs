using HealthCheck.Core.Util.Models;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.ContentPermutation.Models
{
    /// <summary></summary>
    public class HCContentPermutationType
    {
        /// <summary>
        /// Type of your permutation model.
        /// </summary>
        internal Type Type { get; set; }

        /// <summary>
        /// Id of the type.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Optional description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// List of possible permutation instances with ids.
        /// </summary>
        public List<HCContentPermutationChoice> Permutations { get; set; }

        /// <summary>
        /// Property details.
        /// </summary>
        public List<HCBackendInputConfig> PropertyConfigs { get; set; }
    }
}
