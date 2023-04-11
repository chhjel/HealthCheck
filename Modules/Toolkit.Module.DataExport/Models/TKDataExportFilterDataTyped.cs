using QoDL.Toolkit.Module.DataExport.Abstractions;
using System;

namespace QoDL.Toolkit.Module.DataExport.Models
{
    /// <summary>
    /// Filter data passed to <see cref="ITKDataExportStream.GetEnumerableAsync"/> and <see cref="ITKDataExportStream.GetQueryableAsync"/>.
    /// </summary>
    public class TKDataExportFilterDataTyped<TItem, TParameters> : TKDataExportFilterData
    {
        /// <summary>
        /// Query input converted to a predicate if <see cref="ITKDataExportStream.SupportsQuery"/> is true.
        /// </summary>
        public Func<TItem, bool> QueryPredicate { get; set; }

        /// <summary>
        /// Typed custom parameters if <see cref="ITKDataExportStream.CustomParametersType"/> is set.
        /// </summary>
        public TParameters Parameters { get; set; }
    }

    /// <summary>
    /// Filter data passed to <see cref="ITKDataExportStream.GetEnumerableAsync"/> and <see cref="ITKDataExportStream.GetQueryableAsync"/>.
    /// </summary>
    public class TKDataExportFilterDataTyped<TItem> : TKDataExportFilterData
    {
        /// <summary>
        /// Query input converted to a predicate if <see cref="ITKDataExportStream.SupportsQuery"/> is true.
        /// </summary>
        public Func<TItem, bool> QueryPredicate { get; set; }
    }
}
