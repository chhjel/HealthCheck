using HealthCheck.Module.DataExport.Abstractions;

namespace HealthCheck.Module.DataExport
{
    /// <summary>
    /// Options for <see cref="HCDataExportModule"/>.
    /// </summary>
    public class HCDataExportModuleOptions
    {
        /// <summary>
        /// Extra safety measure. Defaults to 1000.
        /// </summary>
        public int MaxPageSize { get; set; } = 1000;

        /// <summary>
        /// Service that handles the data.
        /// </summary>
        public IHCDataExportService Service { get; set; }
    }
}
