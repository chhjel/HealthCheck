using HealthCheck.Module.DataExport.Abstractions;
using System;

namespace HealthCheck.Module.DataExport.Models
{
    /// <summary>
    /// Filter data passed to <see cref="IHCDataExportStream.GetEnumerableAsync"/> and <see cref="IHCDataExportStream.GetQueryableAsync"/>.
    /// </summary>
    public class HCDataExportFilterDataTyped<TItem, TParameters> : HCDataExportFilterData
    {
        /// <summary>
        /// Query input converted to a predicate if <see cref="IHCDataExportStream.SupportsQuery"/> is true.
        /// </summary>
        public Func<TItem, bool> QueryPredicate { get; set; }

        /// <summary>
        /// Typed custom parameters if <see cref="IHCDataExportStream.CustomParametersType"/> is set.
        /// </summary>
        public TParameters Parameters { get; set; }
    }

    /// <summary>
    /// Filter data passed to <see cref="IHCDataExportStream.GetEnumerableAsync"/> and <see cref="IHCDataExportStream.GetQueryableAsync"/>.
    /// </summary>
    public class HCDataExportFilterDataTyped<TItem> : HCDataExportFilterData
    {
        /// <summary>
        /// Query input converted to a predicate if <see cref="IHCDataExportStream.SupportsQuery"/> is true.
        /// </summary>
        public Func<TItem, bool> QueryPredicate { get; set; }
    }
}
