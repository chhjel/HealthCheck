using HealthCheck.Core.Modules.Dataflow.Models;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Dataflow.Abstractions
{
    /// <summary>
    /// A search across multiple streams.
    /// </summary>
    public interface IHCDataflowUnifiedSearch<TAccessRole>
    {
        /// <summary>
        /// Optionally set roles that have access to this stream.
        /// <para>Defaults to null, giving anyone with access to the dataflow page access.</para>
        /// </summary>
        public Maybe<TAccessRole> RolesWithAccess { get; }

        /// <summary>
        /// Name of the search to show in the UI.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Description of the search to show in the UI.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Placeholder for the search input field.
        /// </summary>
        string QueryPlaceholder { get; }

        /// <summary>
        /// Optionally override names of streams per stream type.
        /// </summary>
        Dictionary<Type, string> StreamNamesOverrides { get; }

        /// <summary>
        /// Optionally group the search within the given group name.
        /// </summary>
        string GroupName { get; }

        /// <summary>
        /// Optionally label each grouped item.
        /// <para>[KEY] can be used as a placeholder for the grouped key value.</para>
        /// </summary>
        string GroupByLabel { get; }

        /// <summary>
        /// Optionally override names of streams per stream type within grouped items.
        /// </summary>
        Dictionary<Type, string> GroupByStreamNamesOverrides { get; }

        /// <summary>
        /// True if the search should be visible.
        /// </summary>
        Func<bool> IsVisible { get; }

        /// <summary>
        /// Streams to include in the search.
        /// </summary>
        IEnumerable<Type> StreamTypesToSearch { get; }

        /// <summary>
        /// Create property filter for the given stream and query.
        /// </summary>
        Dictionary<string, string> CreateStreamPropertyFilter(IDataflowStream<TAccessRole> stream, string query);

        /// <summary>
        /// Creates the result data to show for the given entry.
        /// </summary>
        HCDataflowUnifiedSearchResultItem CreateResultItem(Type streamType, IDataflowEntry entry);
    }
}
