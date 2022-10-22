using HealthCheck.Module.DataExport.Abstractions;
using System;

namespace HealthCheck.Module.DataExport.Models
{
    /// <summary>
    /// Filter data passed to <see cref="IHCDataExportStream.GetEnumerableAsync"/> and <see cref="IHCDataExportStream.GetQueryableAsync"/>.
    /// </summary>
    public class HCDataExportFilterData
    {
        /// <summary>
        /// Index of the page to fetch.
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Size of the page to fetch.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Raw query input if <see cref="IHCDataExportStream.SupportsQuery"/> is true.
        /// </summary>
        public string QueryRaw { get; set; }

        /// <summary>
        /// Untyped custom parameters if <see cref="IHCDataExportStream.CustomParametersType"/> is set.
        /// </summary>
        public object ParametersObj { get; set; }
    }
}
