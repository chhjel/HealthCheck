using QoDL.Toolkit.Module.DataExport.Abstractions;
using System;

namespace QoDL.Toolkit.Module.DataExport.Models
{
    /// <summary>
    /// Filter data passed to <see cref="ITKDataExportStream.GetEnumerableAsync"/> and <see cref="ITKDataExportStream.GetQueryableAsync"/>.
    /// </summary>
    public class TKDataExportFilterData
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
        /// Raw query input if <see cref="ITKDataExportStream.SupportsQuery"/> is true.
        /// </summary>
        public string QueryRaw { get; set; }

        /// <summary>
        /// Untyped custom parameters if <see cref="ITKDataExportStream.CustomParametersType"/> is set.
        /// </summary>
        public object ParametersObj { get; set; }
    }
}
