using HealthCheck.Core.Attributes;

namespace HealthCheck.Core.Modules.ContentPermutation.Models
{
    /// <summary></summary>
    public class HCContentPermutationChoiceViewModel
    {
        /// <summary></summary>
        public int Id { get; set; }

        /// <summary></summary>
        public object Choice { get; set; }

        /// <summary></summary>
        [HCRtProperty(ForcedNullable = true)]
        public int? LastRetrievedCount { get; set; }
    }
}
