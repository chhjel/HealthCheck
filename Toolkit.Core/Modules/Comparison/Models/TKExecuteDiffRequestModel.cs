namespace QoDL.Toolkit.Core.Modules.Comparison.Models
{
    /// <summary></summary>
    public class TKExecuteDiffRequestModel
    {
        /// <summary></summary>
        public string HandlerId { get; set; }

        /// <summary></summary>
        public string[] DifferIds { get; set; }

        /// <summary></summary>
        public string LeftId { get; set; }

        /// <summary></summary>
        public string RightId { get; set; }
    }
}
