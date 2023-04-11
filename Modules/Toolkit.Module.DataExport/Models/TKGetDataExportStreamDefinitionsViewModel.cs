using System.Collections.Generic;

namespace QoDL.Toolkit.Module.DataExport.Models
{
    /// <summary></summary>
    public class TKGetDataExportStreamDefinitionsViewModel
    {
        /// <summary></summary>
        public bool SupportsStorage { get; set; }

        /// <summary></summary>
        public List<TKDataExportStreamViewModel> Streams { get; set; } = new();

        /// <summary></summary>
        public List<TKDataExportExporterViewModel> Exporters { get; set; } = new();
    }
}
