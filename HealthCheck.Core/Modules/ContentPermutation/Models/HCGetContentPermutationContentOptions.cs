using HealthCheck.Core.Modules.ContentPermutation.Abstractions;
using HealthCheck.Core.Modules.ContentPermutation.Attributes;
using System;

namespace HealthCheck.Core.Modules.ContentPermutation.Models
{
    /// <summary>
    /// Data sent to <see cref="IHCContentPermutationContentHandler"/>.
    /// </summary>
    public class HCGetContentPermutationContentOptions
    {
        /// <summary>
        /// Type of permutation, a class decorated with <see cref="HCContentPermutationTypeAttribute"/>.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// An instance of a class decorated with <see cref="HCContentPermutationTypeAttribute"/>.
        /// </summary>
        public object PermutationObj { get; set; }

        /// <summary>
        /// Max count to retrieve.
        /// </summary>
        public int MaxCount { get; set; }
    }

    /// <summary>
    /// Data sent to <see cref="HCContentPermutationContentHandlerBase{TPermutation}"/>.
    /// </summary>
    public class HCGetContentPermutationContentOptions<TPermutation> : HCGetContentPermutationContentOptions
    {
        /// <summary>
        /// An instance of a class decorated with <see cref="HCContentPermutationTypeAttribute"/>, with correct type.
        /// </summary>
        public TPermutation Permutation { get; set; }
    }
}
