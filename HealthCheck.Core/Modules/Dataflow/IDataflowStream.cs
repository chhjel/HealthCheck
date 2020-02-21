using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Dataflow
{
    /// <summary>
    /// A stream that can return data to display.
    /// </summary>
    public interface IDataflowStream
    {
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
        /// True if the stream should be visible.
        /// </summary>
        Func<bool> IsVisible { get; }

        /// <summary>
        /// True if the stream supports datetime filtering in <see cref="GetLatestStreamEntriesAsync"/>.
        /// </summary>
        bool SupportsFilterByDate { get; }

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
