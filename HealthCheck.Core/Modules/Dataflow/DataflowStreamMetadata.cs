using HealthCheck.Core.Util;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Dataflow
{
    /// <summary>
    /// Metadata describing an <see cref="IDataflowStream{TAccessRole}"/>.
    /// </summary>
    public class DataflowStreamMetadata<TAccessRole>
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
        /// True if the stream supports datetime filtering in <see cref="IDataflowStream{TAccessRole}.GetLatestStreamEntriesAsync(DataflowStreamFilter)"/>.
        /// </summary>
        public bool SupportsFilterByDate { get; set; }

        /// <summary>
        /// Display options for properties.
        /// </summary>
        public List<DataFlowPropertyDisplayInfo> PropertyDisplayInfo { get; set; }

        /// <summary>
        /// Optionally set roles that have access to this stream.
        /// </summary>
        public Maybe<TAccessRole> RolesWithAccess { get; set; }
    }
}
