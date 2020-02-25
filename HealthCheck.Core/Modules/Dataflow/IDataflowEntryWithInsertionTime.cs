using System;

namespace HealthCheck.Core.Modules.Dataflow
{
    /// <summary>
    /// A <see cref="IDataflowEntry"/> with an <see cref="InsertionTime"/> property.
    /// </summary>
    public interface IDataflowEntryWithInsertionTime : IDataflowEntry
    {
        /// <summary>
        /// Time of insertion.
        /// </summary>
        DateTime? InsertionTime { get; set; }
    }
}
