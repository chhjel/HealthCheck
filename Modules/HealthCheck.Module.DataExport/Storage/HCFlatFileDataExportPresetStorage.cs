using HealthCheck.Core.Config;
using HealthCheck.Core.Util;
using HealthCheck.Module.DataExport.Abstractions;
using HealthCheck.Module.DataExport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace HealthCheck.Module.DataExport.Storage
{
    /// <summary></summary>
    public class HCFlatFileDataExportPresetStorage : IHCDataExportPresetStorage
    {
        private HCSimpleDataStoreWithId<HCDataExportStreamQueryPreset, Guid> Store { get; set; }

        /// <summary>
        /// Create a new <see cref="HCFlatFileDataExportPresetStorage"/> with the given file path.
        /// </summary>
        /// <param name="filepath">Filepath to where the data will be stored.</param>
        public HCFlatFileDataExportPresetStorage(string filepath)
        {
            Store = new HCSimpleDataStoreWithId<HCDataExportStreamQueryPreset, Guid>(
                filepath,
                serializer: new Func<HCDataExportStreamQueryPreset, string>(
                    (e) => HCGlobalConfig.Serializer.Serialize(e, pretty: false)),
                deserializer: new Func<string, HCDataExportStreamQueryPreset>(
                    (row) => HCGlobalConfig.Serializer.Deserialize(row, typeof(HCDataExportStreamQueryPreset)) as HCDataExportStreamQueryPreset),
                idSelector: (e) => e.Id,
                idSetter: (e, id) => e.Id = id,
                nextIdFactory: (events, e) => Guid.NewGuid()
            );
        }

        /// <inheritdoc />
        public Task<IEnumerable<HCDataExportStreamQueryPreset>> GetStreamQueryPresetsAsync(string streamId)
            => Task.FromResult(Store.GetEnumerable().Where(x => x.StreamId == streamId));

        /// <inheritdoc />
        public Task<HCDataExportStreamQueryPreset> GetStreamQueryPresetAsync(Guid id)
            => Task.FromResult(Store.GetEnumerable().FirstOrDefault(x => x.Id == id));

        /// <inheritdoc />
        public Task DeleteStreamQueryPresetAsync(Guid id)
        {
            Store.DeleteItem(id);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<HCDataExportStreamQueryPreset> SaveStreamQueryPresetAsync(HCDataExportStreamQueryPreset preset)
        {
            var item = Store.InsertOrUpdateItem(preset);
            return Task.FromResult(item);
        }
    }
}
