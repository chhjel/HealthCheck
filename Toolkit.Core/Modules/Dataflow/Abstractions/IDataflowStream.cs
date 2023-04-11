using QoDL.Toolkit.Core.Modules.Dataflow.Models;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.Dataflow.Abstractions
{
    /// <summary>
    /// A stream that can return data to display.
    /// </summary>
    public interface IDataflowStream<TAccessRole>
    {
        /// <summary>
        /// Optionally set roles that have access to this stream.
        /// <para>Defaults to null, giving anyone with access to the dataflow page access.</para>
        /// </summary>
        public Maybe<TAccessRole> RolesWithAccess { get; }

        /// <summary>
        /// Unique id of the stream.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Name of the stream to show in the UI.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Description of the stream to show in the UI.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Optionally group the stream within the given group name.
        /// </summary>
        string GroupName { get; }

        /// <summary>
        /// True if the stream should be visible.
        /// </summary>
        Func<bool> IsVisible { get; }

        /// <summary>
        /// True if the stream supports datetime filtering in <see cref="GetLatestStreamEntriesAsync"/>.
        /// </summary>
        bool SupportsFilterByDate { get; }

        /// <summary>
        /// Optional name of a <see cref="DateTime"/> or <see cref="DateTimeOffset"/> property that will be used for grouping in frontend.
        /// </summary>
        string DateTimePropertyNameForUI { get; set; }

        /// <summary>
        /// Returns the latest entries filtered.
        /// </summary>
        Task<IEnumerable<IDataflowEntry>> GetLatestStreamEntriesAsync(DataflowStreamFilter filter);

        /// <summary>
        /// Get information about the properties to display.
        /// </summary>
        IEnumerable<DataFlowPropertyDisplayInfo> GetEntryPropertiesInfo();
    }
}
