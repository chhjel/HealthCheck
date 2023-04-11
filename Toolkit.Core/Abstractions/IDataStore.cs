using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Abstractions
{
    /// <summary>
    /// A generic object storage.
    /// </summary>
    public interface IDataStore<out TEntry>
    {
        /// <summary>
        /// Get an enumerable of the stored objects.
        /// </summary>
        IEnumerable<TEntry> GetEnumerable();
    }
}
