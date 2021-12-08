namespace HealthCheck.Module.DataExport.Models
{
    /// <summary></summary>
    public class HCDataExportStreamViewModel
    {
        /// <summary></summary>
        public string Id { get; set; }

        /// <summary></summary>
        public string Name { get; set; }

        /// <summary></summary>
        public string Description { get; set; }

        /// <summary></summary>
        public string GroupName { get; set; }

        /// <summary></summary>
        public HCDataExportStreamItemDefinitionViewModel ItemDefinition { get; set; }
    }
}
