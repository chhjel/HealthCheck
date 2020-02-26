using HealthCheck.Core.Modules.Dataflow;
using System.Collections.Generic;

namespace HealthCheck.Core.Abstractions
{
    /// <summary>
    /// A generic object storage.
    /// </summary>
    public interface IDataStore<TEntry>
    {
        /// <summary>
        /// Get an enumerable of the stored objects.
        /// </summary>
        IEnumerable<TEntry> GetEnumerable();
    }
}
