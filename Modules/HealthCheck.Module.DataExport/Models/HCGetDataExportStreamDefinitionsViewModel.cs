using System.Collections.Generic;

namespace HealthCheck.Core.Modules.DataExport.Models
{
    /// <summary></summary>
    public class HCGetDataExportStreamDefinitionsViewModel
    {
        /// <summary></summary>
        public List<HCDataExportStreamViewModel> Streams { get; set; } = new();
    }
}
