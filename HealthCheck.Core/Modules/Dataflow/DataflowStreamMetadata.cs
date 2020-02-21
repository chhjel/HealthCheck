using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Dataflow
{
    /// <summary>
    /// Metadata describing an <see cref="IDataflowStream"/>.
    /// </summary>
    public class DataflowStreamMetadata
    {
        /// <summary>
        /// Unique id of the stream.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the stream to show in the UI.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the stream to show in the UI.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// True if the stream supports datetime filtering in <see cref="IDataflowStream.GetLatestStreamEntriesAsync(DataflowStreamFilter)"/>.
        /// </summary>
        public bool SupportsFilterByDate { get; set; }

        /// <summary>
        /// Display options for properties.
        /// </summary>
        public List<DataFlowPropertyDisplayInfo> PropertyDisplayInfo { get; set; }
    }
}
