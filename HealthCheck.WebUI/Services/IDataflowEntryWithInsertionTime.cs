using HealthCheck.Core.Modules.Dataflow;
using System;

namespace HealthCheck.WebUI.Services
{
    /// <summary>
    /// A <see cref="IDataflowEntry"/> with an <see cref="InsertionTime"/> property.
    /// <para>Used in <see cref="FlatFileStoredDataflowStream{TEntry, TEntryId}"/>.</para>
    /// </summary>
    public interface IDataflowEntryWithInsertionTime : IDataflowEntry
    {
        /// <summary>
        /// Time of insertion.
        /// </summary>
        DateTime? InsertionTime { get; set; }
    }
}
