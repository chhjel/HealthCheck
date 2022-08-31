namespace HealthCheck.Core.Modules.GoTo.Models
{
    /// <summary></summary>
    public class HCGoToResolvedDataWithResolverId
    {
        /// <summary></summary>
        public string ResolverId { get; set; }
        /// <summary></summary>
        public HCGoToResolvedData Data { get; set; }
        /// <summary></summary>
        public string Error { get; set; }
    }
}
