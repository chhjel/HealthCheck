using Newtonsoft.Json;
using QoDL.Toolkit.Core.Modules.EventNotifications.Abstractions;
using QoDL.Toolkit.Core.Modules.EventNotifications.Models;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.WebUI.Services;

/// <summary>
/// Stores and retrieves <see cref="EventSinkNotificationConfig"/>s.
/// </summary>
public class FlatFileEventSinkNotificationConfigStorage : IEventSinkNotificationConfigStorage
{
    private TKSimpleDataStoreWithId<EventSinkNotificationConfig, Guid> Store { get; set; }

    private static readonly object _cacheUpdateLock = new();
    private static Dictionary<string, IEnumerable<EventSinkNotificationConfig>> ConfigCache { get; set; } = new Dictionary<string, IEnumerable<EventSinkNotificationConfig>>();
    private string CacheKey => Store.FilePath.ToLower();
    private static Dictionary<string, EventSinkNotificationConfig> SaveBuffer { get; set; } = new Dictionary<string, EventSinkNotificationConfig>();
    private static bool IsWriteQueued { get; set; }
    private readonly float WriteDelay = 2f;

    /// <summary>
    /// Create a new <see cref="FlatFileEventSinkNotificationConfigStorage"/> with the given file path.
    /// </summary>
    /// <param name="filepath">Filepath to where the data will be stored.</param>
    public FlatFileEventSinkNotificationConfigStorage(string filepath)
    {
        Store = new TKSimpleDataStoreWithId<EventSinkNotificationConfig, Guid>(
            filepath,
            serializer: new Func<EventSinkNotificationConfig, string>((e) => JsonConvert.SerializeObject(e)),
            deserializer: new Func<string, EventSinkNotificationConfig>((row) => JsonConvert.DeserializeObject<EventSinkNotificationConfig>(row)),
            idSelector: (e) => e.Id,
            idSetter: (e, id) => e.Id = id,
            nextIdFactory: (events, e) => (e.Id == Guid.Empty ? Guid.NewGuid() : e.Id)
        );
    }

    /// <summary>
    /// Get all configs.
    /// </summary>
    public IEnumerable<EventSinkNotificationConfig> GetConfigs()
    {
        lock (_cacheUpdateLock)
        {
            var key = CacheKey;
            if (!ConfigCache.ContainsKey(key))
            {
                ConfigCache[key] = Store.GetEnumerable().ToList();
            }

            var cachedConfigs = ConfigCache[key];

            return SaveBuffer
                .Values
                .Union(cachedConfigs.Where(x => !SaveBuffer.Any(s => s.Value.Id == x.Id)))
                .ToArray();
        }
    }

    /// <summary>
    /// Inserts or updates the given config.
    /// </summary>
    public EventSinkNotificationConfig SaveConfig(EventSinkNotificationConfig config)
    {
        lock (_cacheUpdateLock)
        {
            var id = config.Id == Guid.Empty ? Guid.NewGuid() : config.Id;
            config.Id = id;

            SaveBuffer[id.ToString()] = config;

            QueueWriteBufferToFile();

            return config;
        }
    }

    /// <summary>
    /// Deletes the config with the given id.
    /// </summary>
    public void DeleteConfig(Guid configId)
    {
        lock (_cacheUpdateLock)
        {
            if (SaveBuffer.ContainsKey(configId.ToString()))
            {
                SaveBuffer.Remove(configId.ToString());
            }

            var cacheKey = CacheKey;
            if (ConfigCache.ContainsKey(cacheKey))
            {
                ConfigCache[cacheKey] = ConfigCache[cacheKey]
                    .Where(x => x?.Id != configId)
                    .ToList();
            }
        }
        Store.DeleteItem(configId);
    }

    /// <summary>
    /// Deconstructor. Stores any buffered data before self destructing.
    /// </summary>
    ~FlatFileEventSinkNotificationConfigStorage()
    {
        WriteBufferToFile();
    }

    private void QueueWriteBufferToFile()
    {
        lock (_cacheUpdateLock)
        {
            // Return if already queued a write
            if (IsWriteQueued)
            {
                return;
            }
            IsWriteQueued = true;
        }

        // Wait to write
        Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(WriteDelay));
            WriteBufferToFile();
        });
    }

    internal void WriteBufferToFile()
    {
        try
        {
            lock (_cacheUpdateLock)
            {
                if (SaveBuffer.Count == 0)
                {
                    IsWriteQueued = false;
                    return;
                }

                foreach (var config in SaveBuffer.Values)
                {
                    Store.InsertOrUpdateItem(config, (old) =>
                    {
                        config.LatestResults = config
                            ?.LatestResults
                            ?.Union(old?.LatestResults ?? Enumerable.Empty<string>())
                            ?.Take(10)
                            ?.ToList()
                            ?? new List<string>();
                        return config;
                    });
                }

                SaveBuffer.Clear();
                var key = CacheKey;
                ConfigCache.Remove(key);
            }
        }
        catch (Exception) { /* Ignore error here */ }
        IsWriteQueued = false;
    }
}
