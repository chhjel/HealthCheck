using QoDL.Toolkit.Core.Attributes;

namespace QoDL.Toolkit.Core.Modules.ContentPermutation.Models
{
    /// <summary></summary>
    public class TKContentPermutationChoiceViewModel
    {
        /// <summary></summary>
        public int Id { get; set; }

        /// <summary></summary>
        public object Choice { get; set; }

        /// <summary></summary>
        [TKRtProperty(ForcedNullable = true)]
        public int? LastRetrievedCount { get; set; }
    }
}
