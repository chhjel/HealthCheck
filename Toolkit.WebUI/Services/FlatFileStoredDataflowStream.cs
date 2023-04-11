using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Modules.Dataflow.Abstractions;
using QoDL.Toolkit.Core.Util;
using Newtonsoft.Json;
using System;

namespace QoDL.Toolkit.WebUI.Services;

/// <summary>
/// A built in dataflow stream that stores and retrieves entries from a flatfile.
/// </summary>
public abstract class FlatFileStoredDataflowStream<TAccessRole, TEntry, TEntryId> : StoredDataflowStream<TAccessRole, TEntry>
    where TEntry : IDataflowEntryWithInsertionTime
{
    /// <summary>
    /// A built in dataflow stream that stores and retrieves entries from a flatfile.
    /// </summary>
    protected FlatFileStoredDataflowStream(
        string filepath,
        Func<TEntry, TEntryId> idSelector,
        Action<TEntry, TEntryId> idSetter,
        TimeSpan? maxEntryAge = null
    ) : base(CreateDataStore(filepath, idSelector, idSetter, maxEntryAge))
    {
    }

    private static IDataStoreWithEntryId<TEntry> CreateDataStore(
        string filepath, Func<TEntry, TEntryId> idSelector, Action<TEntry, TEntryId> idSetter, TimeSpan? maxEntryAge)
    {
        var store = new TKSimpleDataStoreWithId<TEntry, TEntryId>(
            filepath,
            serializer: new Func<TEntry, string>((e) => JsonConvert.SerializeObject(e)),
            deserializer: new Func<string, TEntry>((row) => JsonConvert.DeserializeObject<TEntry>(row)),
            idSelector: idSelector,
            idSetter: idSetter,
            nextIdFactory: (entries, e) => idSelector(e)
        );

        if (maxEntryAge != null)
        {
            var minimumCleanupInterval = TimeSpan.FromMinutes(30);
            store.RetentionOptions = new TKStorageRetentionOptions<TEntry>(
                (e) => e.InsertionTime ?? DateTimeOffset.MinValue,
                maxAge: maxEntryAge.Value,
                minimumCleanupInterval: (maxEntryAge.Value < minimumCleanupInterval) ? maxEntryAge.Value : minimumCleanupInterval,
                delayFirstCleanup: false
            );
        }

        return store;
    }
}
