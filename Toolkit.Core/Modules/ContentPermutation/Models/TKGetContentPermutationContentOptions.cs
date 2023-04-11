using QoDL.Toolkit.Core.Modules.ContentPermutation.Abstractions;
using QoDL.Toolkit.Core.Modules.ContentPermutation.Attributes;
using System;

namespace QoDL.Toolkit.Core.Modules.ContentPermutation.Models
{
    /// <summary>
    /// Data sent to <see cref="ITKContentPermutationContentHandler"/>.
    /// </summary>
    public class TKGetContentPermutationContentOptions
    {
        /// <summary>
        /// Type of permutation, a class decorated with <see cref="TKContentPermutationTypeAttribute"/>.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// An instance of a class decorated with <see cref="TKContentPermutationTypeAttribute"/>.
        /// </summary>
        public object PermutationObj { get; set; }

        /// <summary>
        /// Max count to retrieve.
        /// </summary>
        public int MaxCount { get; set; }
    }

    /// <summary>
    /// Data sent to <see cref="TKContentPermutationContentHandlerBase{TPermutation}"/>.
    /// </summary>
    public class TKGetContentPermutationContentOptions<TPermutation> : TKGetContentPermutationContentOptions
    {
        /// <summary>
        /// An instance of a class decorated with <see cref="TKContentPermutationTypeAttribute"/>, with correct type.
        /// </summary>
        public TPermutation Permutation { get; set; }
    }
}
