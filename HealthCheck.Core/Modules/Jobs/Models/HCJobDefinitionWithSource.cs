namespace HealthCheck.Core.Modules.Jobs.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class HCJobDefinitionWithSource
    {
        /// <summary></summary>
        public string SourceId { get; set; }

        /// <summary></summary>
        public HCJobDefinition Definition { get; set; }
    }
}
