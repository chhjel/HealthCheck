using Newtonsoft.Json;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Module.IPWhitelist.Abstractions;
using QoDL.Toolkit.Module.IPWhitelist.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.IPWhitelist.Storage;

/// <summary></summary>
public class TKIPWhitelistConfigFlatFileStorage : ITKIPWhitelistConfigStorage
{
    private TKSimpleDataStoreWithId<TKIPWhitelistConfig, Guid> Store { get; }
    private TKIPWhitelistConfig _cache;
    private readonly object _cacheLock = new();

    /// <summary>
    /// Create a new <see cref="TKIPWhitelistConfigFlatFileStorage"/> with the given file path.
    /// </summary>
    /// <param name="filepath">Filepath to where the data will be stored.</param>
    public TKIPWhitelistConfigFlatFileStorage(string filepath)
    {
        Store = new TKSimpleDataStoreWithId<TKIPWhitelistConfig, Guid>(
            filepath,
            serializer: new Func<TKIPWhitelistConfig, string>(JsonConvert.SerializeObject),
            deserializer: new Func<string, TKIPWhitelistConfig>((row) => JsonConvert.DeserializeObject<TKIPWhitelistConfig>(row)),
            idSelector: (x) => Guid.Empty,
            idSetter: (x, id) => { },
            nextIdFactory: (all, x) => Guid.NewGuid()
        );
    }

    /// <inheritdoc />
    public Task<TKIPWhitelistConfig> GetConfigAsync()
    {
        lock (_cacheLock)
        {
            if (_cache != null)
            {
                return Task.FromResult(_cache);
            }

            var config = Store.GetEnumerable().FirstOrDefault();
            _cache = config;
            return Task.FromResult(config);
        }
    }

    /// <inheritdoc />
    public Task SaveConfigAsync(TKIPWhitelistConfig config)
    {
        var updatedConfig = Store.InsertOrUpdateItem(config);
        lock (_cacheLock)
        {
            _cache = updatedConfig;
        }
        return Task.CompletedTask;
    }
}
