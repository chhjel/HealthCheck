using QoDL.Toolkit.Core.Util.Models;
using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.ContentPermutation.Models
{
    /// <summary></summary>
    public class TKContentPermutationType
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
        /// Requested <see cref="TKGetContentPermutationContentOptions.MaxCount"/> will be limited by this number.
        /// </summary>
        public int MaxAllowedContentCount { get; set; }

        /// <summary>
        /// Default value for <see cref="TKGetContentPermutationContentOptions.MaxCount"/> will be limited by this number.
        /// </summary>
        public int DefaultContentCount { get; set; }

        /// <summary>
        /// List of possible permutation instances with ids.
        /// </summary>
        public List<TKContentPermutationChoice> Permutations { get; set; }

        /// <summary>
        /// Property details.
        /// </summary>
        public List<TKBackendInputConfig> PropertyConfigs { get; set; }
    }
}
