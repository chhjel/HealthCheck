using QoDL.Toolkit.Core.Util.Models;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.ContentPermutation.Models
{
    /// <summary></summary>
    public class TKContentPermutationTypeViewModel
    {
        /// <summary></summary>
        public string Id { get; set; }

        /// <summary></summary>
        public string Name { get; set; }

        /// <summary></summary>
        public string Description { get; set; }

        /// <summary></summary>
        public int MaxAllowedContentCount { get; set; }

        /// <summary></summary>
        public int DefaultContentCount { get; set; }

        /// <summary></summary>
        public List<TKContentPermutationChoiceViewModel> Permutations { get; set; }

        /// <summary>
        /// Property details.
        /// </summary>
        public List<TKBackendInputConfig> PropertyConfigs { get; set; }
    }
}
