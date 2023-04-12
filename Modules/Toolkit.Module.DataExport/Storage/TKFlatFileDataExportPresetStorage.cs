using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Module.DataExport.Abstractions;
using QoDL.Toolkit.Module.DataExport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.DataExport.Storage;

/// <summary></summary>
public class TKFlatFileDataExportPresetStorage : ITKDataExportPresetStorage
{
    private TKSimpleDataStoreWithId<TKDataExportStreamQueryPreset, Guid> Store { get; set; }

    /// <summary>
    /// Create a new <see cref="TKFlatFileDataExportPresetStorage"/> with the given file path.
    /// </summary>
    /// <param name="filepath">Filepath to where the data will be stored.</param>
    public TKFlatFileDataExportPresetStorage(string filepath)
    {
        Store = new TKSimpleDataStoreWithId<TKDataExportStreamQueryPreset, Guid>(
            filepath,
            serializer: new Func<TKDataExportStreamQueryPreset, string>(
                (e) => TKGlobalConfig.Serializer.Serialize(e, pretty: false)),
            deserializer: new Func<string, TKDataExportStreamQueryPreset>(
                (row) => TKGlobalConfig.Serializer.Deserialize(row, typeof(TKDataExportStreamQueryPreset)) as TKDataExportStreamQueryPreset),
            idSelector: (e) => e.Id,
            idSetter: (e, id) => e.Id = id,
            nextIdFactory: (events, e) => Guid.NewGuid()
        );
    }

    /// <inheritdoc />
    public Task<IEnumerable<TKDataExportStreamQueryPreset>> GetStreamQueryPresetsAsync(string streamId)
        => Task.FromResult(Store.GetEnumerable().Where(x => x.StreamId == streamId));

    /// <inheritdoc />
    public Task<TKDataExportStreamQueryPreset> GetStreamQueryPresetAsync(Guid id)
        => Task.FromResult(Store.GetEnumerable().FirstOrDefault(x => x.Id == id));

    /// <inheritdoc />
    public Task DeleteStreamQueryPresetAsync(Guid id)
    {
        Store.DeleteItem(id);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<TKDataExportStreamQueryPreset> SaveStreamQueryPresetAsync(TKDataExportStreamQueryPreset preset)
    {
        var item = Store.InsertOrUpdateItem(preset);
        return Task.FromResult(item);
    }
}
