namespace QoDL.Toolkit.Core.Modules.Jobs.Models
{
    /// <summary></summary>
    public class TKJobsGetPagedHistoryRequestModel
    {
        /// <summary></summary>
        public string SourceId { get; set; }
        /// <summary></summary>
        public string JobId { get; set; }
        /// <summary></summary>
        public int PageIndex { get; set; }
        /// <summary></summary>
        public int PageSize { get; set; }
    }
}
