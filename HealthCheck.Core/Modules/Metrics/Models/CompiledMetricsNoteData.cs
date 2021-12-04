using System;

namespace HealthCheck.Core.Modules.Metrics.Models
{
    /// <summary>
    /// Metrics data prepared for frontend consumption.
    /// </summary>
    public class CompiledMetricsNoteData
    {
        /// <summary>
        /// Id of this note.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Note contents.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// When the latest change was reported.
        /// </summary>
        public DateTimeOffset LastChanged { get; set; }

        /// <summary></summary>
        public override string ToString() => $"{Id}: {Note}";
    }
}
