namespace HealthCheck.Core.Modules.ContentPermutation.Models
{
    /// <summary></summary>
    public class HCGetPermutatedContentRequest
    {
        /// <summary></summary>
        public int MaxCount { get; set; }

        /// <summary></summary>
        public string PermutationTypeId { get; set; }

        /// <summary></summary>
        public int PermutationChoiceId { get; set; }
    }
}
