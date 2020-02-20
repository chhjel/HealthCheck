using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Dataflow
{
    /// <summary>
    /// Filter options for <see cref="IDataflowStream.GetLatestStreamEntriesAsync(DataflowStreamFilter)"/>.
    /// </summary>
    public class DataflowStreamFilter
    {
        /// <summary>
        /// Number of entries to skip.
        /// </summary>
        public int Skip { get; set; }

        /// <summary>
        /// Number of entries to take.
        /// </summary>
        public int Take { get; set; }

        /// <summary>
        /// If not null, only include from the given date.
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// If not null, only include up until the given date.
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Filter on property values.
        /// <para>Keys are property names and values are input values.</para>
        /// </summary>
        public Dictionary<string, string> PropertyFilters { get; set; }
    }

}
