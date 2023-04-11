using System;

namespace QoDL.Toolkit.Core.Modules.Dataflow.Abstractions
{
    /// <summary>
    /// A <see cref="IDataflowEntry"/> with an <see cref="InsertionTime"/> property.
    /// </summary>
    public interface IDataflowEntryWithInsertionTime : IDataflowEntry
    {
        /// <summary>
        /// Time of insertion.
        /// </summary>
        DateTimeOffset? InsertionTime { get; set; }
    }
}
