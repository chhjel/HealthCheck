namespace HealthCheck.Core.Modules.ContentPermutation.Models
{
    /// <summary></summary>
    public class HCContentPermutationChoice
    {
        /// <summary>
        /// Id of the choice.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Instance of your model with permuted values.
        /// </summary>
        public object Choice { get; set; }
    }
}
