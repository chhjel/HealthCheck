using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Module.DataExport.Abstractions
{
    /// <summary>
    /// Stream of items that can be filtered and exported upon.
    /// <para>Base class for stronger typing.</para>
    /// </summary>
    public abstract class HCDataExportStreamBase<TItem> : IHCDataExportStream
    {
        /// <inheritdoc />
        public Type ItemType => typeof(TItem);

        /// <inheritdoc />
        public abstract string StreamDisplayName { get; }

        /// <inheritdoc />
        public abstract string StreamDescription { get; }

        /// <inheritdoc />
        public virtual string StreamGroupName { get; }

        /// <inheritdoc />
        public virtual object AllowedAccessRoles { get; }

        /// <inheritdoc />
        public virtual List<string> Categories { get; }

        /// <inheritdoc />
        public abstract int ExportBatchSize { get; }

        /// <inheritdoc />
        public virtual async Task<IQueryable> GetQueryableAsync()
            => await GetQueryableItemsAsync();

        /// <summary>
        /// Get items to be filtered and exported.
        /// </summary>
        protected abstract Task<IQueryable<TItem>> GetQueryableItemsAsync();
    }
}
