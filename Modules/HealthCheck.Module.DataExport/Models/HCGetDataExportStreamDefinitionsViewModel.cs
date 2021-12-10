using System.Collections.Generic;

namespace HealthCheck.Module.DataExport.Models
{
    /// <summary></summary>
    public class HCGetDataExportStreamDefinitionsViewModel
    {
        /// <summary></summary>
        public bool SupportsStorage { get; set; }

        /// <summary></summary>
        public List<HCDataExportStreamViewModel> Streams { get; set; } = new();
    }
}
