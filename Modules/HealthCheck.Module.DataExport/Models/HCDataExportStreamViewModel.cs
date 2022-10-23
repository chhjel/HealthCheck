using HealthCheck.Core.Util.Models;
using System.Collections.Generic;

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
        public bool ShowQueryInput { get; set; }

        /// <summary></summary>
        public bool AllowAnyPropertyName { get; set; }

        /// <summary></summary>
        public HCDataExportStreamItemDefinitionViewModel ItemDefinition { get; set; }

        /// <summary></summary>
        public List<HCBackendInputConfig> CustomParameterDefinitions { get; set; }

        /// <summary></summary>
        public List<HCDataExportValueFormatterViewModel> ValueFormatters { get; set; }
    }
}
