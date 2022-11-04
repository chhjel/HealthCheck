namespace HealthCheck.Core.Modules.Jobs.Models
{
    /// <summary></summary>
    public class HCJobsGetPagedHistoryRequestModel
    {
        /// <summary></summary>
        public string JobId { get; set; }
        /// <summary></summary>
        public int PageIndex { get; set; }
        /// <summary></summary>
        public int PageSize { get; set; }
    }
}
