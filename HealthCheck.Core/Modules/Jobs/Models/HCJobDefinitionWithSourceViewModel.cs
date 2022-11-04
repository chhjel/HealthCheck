namespace HealthCheck.Core.Modules.Jobs.Models
{
    /// <summary></summary>
    public class HCJobDefinitionWithSourceViewModel
    {
        /// <summary></summary>
        public string SourceId { get; set; }

        /// <summary></summary>
        public HCJobDefinitionViewModel Definition { get; set; }
    }
}
