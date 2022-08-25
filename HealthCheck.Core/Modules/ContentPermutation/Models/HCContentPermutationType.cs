using HealthCheck.Core.Modules.ContentPermutation.Attributes;
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
        /// Property details for any decorated with <see cref="HCContentPermutationPropertyAttribute"/>.
        /// </summary>
        public Dictionary<string, HCContentPermutationPropertyDetails> PropertyDetails { get; set; }
    }
}
