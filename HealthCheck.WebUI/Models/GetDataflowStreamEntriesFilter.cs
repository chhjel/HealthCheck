using HealthCheck.Core.Modules.Dataflow;

namespace HealthCheck.WebUI.Models
{
    /// <summary>
    /// Model sent to <see cref="HealthCheck.WebUI.Abstractions.HealthCheckControllerBase{TAccessRole}.GetDataflowStreamEntries(GetDataflowStreamEntriesFilter)"/>
    /// </summary>
    public class GetDataflowStreamEntriesFilter
    {
        /// <summary>
        /// Id of the stream to get data from.
        /// </summary>
        public string StreamId { get; set; }

        /// <summary>
        /// Filter passed to the stream.
        /// </summary>
        public DataflowStreamFilter StreamFilter { get; set; }
    }
}
