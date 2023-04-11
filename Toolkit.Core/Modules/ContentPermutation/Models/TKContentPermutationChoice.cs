namespace QoDL.Toolkit.Core.Modules.ContentPermutation.Models
{
    /// <summary></summary>
    public class TKContentPermutationChoice
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
