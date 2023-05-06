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
        => Task.FromResult(Store.GetEnumerable().FirstOrDefault());

    /// <inheritdoc />
    public Task SaveConfigAsync(TKIPWhitelistConfig config)
    {
        Store.InsertOrUpdateItem(config);
        return Task.CompletedTask;
    }
}
