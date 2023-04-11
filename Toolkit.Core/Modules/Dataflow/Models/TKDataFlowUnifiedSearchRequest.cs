namespace QoDL.Toolkit.Core.Modules.Dataflow.Models
{
    /// <summary>
    /// Model sent to unified search
    /// </summary>
    public class TKDataFlowUnifiedSearchRequest
    {
        /// <summary>
        /// Id of the search to get data from.
        /// </summary>
        public string SearchId { get; set; }

        /// <summary></summary>
        public string Query { get; set; }

        /// <summary></summary>
        public int PageIndex { get; set; }

        /// <summary></summary>
        public int PageSize { get; set; }
    }
}
