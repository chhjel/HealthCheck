#if NETFULL || NETCORE
using HealthCheck.Core.Util;
using HealthCheck.Module.DynamicCodeExecution.Abstractions;
using HealthCheck.Module.DynamicCodeExecution.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Module.DynamicCodeExecution.Storage
{
    /// <summary>
    /// Stores and retrieves <see cref="DynamicCodeScript"/>s.
    /// </summary>
    public class FlatFileDynamicCodeScriptStorage : IDynamicCodeScriptStorage
    {
        private HCSimpleDataStoreWithId<DynamicCodeScript, Guid> Store { get; set; }

        /// <summary>
        /// Create a new <see cref="FlatFileDynamicCodeScriptStorage"/> with the given file path.
        /// </summary>
        /// <param name="filepath">Filepath to where the data will be stored.</param>
        public FlatFileDynamicCodeScriptStorage(string filepath)
        {
            Store = new HCSimpleDataStoreWithId<DynamicCodeScript, Guid>(
                filepath,
                serializer: new Func<DynamicCodeScript, string>((e) => JsonConvert.SerializeObject(e)),
                deserializer: new Func<string, DynamicCodeScript>((row) => JsonConvert.DeserializeObject<DynamicCodeScript>(row)),
                idSelector: (e) => e.Id,
                idSetter: (e, id) => e.Id = id,
                nextIdFactory: (events, e) => (e.Id == Guid.Empty ? Guid.NewGuid() : e.Id)
            );
        }

        /// <summary>
        /// Get all stored scripts.
        /// </summary>
        public async Task<List<DynamicCodeScript>> GetAllScripts()
            => await Task.FromResult(Store.GetEnumerable().ToList());

        /// <summary>
        /// Get a single stored script.
        /// </summary>
        public async Task<DynamicCodeScript> GetScript(Guid id)
            => await Task.FromResult(Store.GetItem(id));

        /// <summary>
        /// Deletes a single stored script.
        /// </summary>
        public async Task<bool> DeleteScript(Guid id)
        {
            Store.DeleteItem(id);
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Save or create the given script and return the script with any changes.
        /// </summary>
        public async Task<DynamicCodeScript> SaveScript(DynamicCodeScript script)
            => await Task.FromResult(Store.InsertOrUpdateItem(script));
    }
}
#endif
