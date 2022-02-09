using HealthCheck.Core.Modules.SecureFileDownload.Abstractions;
using HealthCheck.Core.Modules.SecureFileDownload.Models;
using HealthCheck.Core.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.WebUI.Services
{
    /// <summary>
    /// Stores and retrieves <see cref="SecureFileDownloadDefinition"/>s.
    /// </summary>
    public class FlatFileSecureFileDownloadDefinitionStorage : ISecureFileDownloadDefinitionStorage
    {
        private HCSimpleDataStoreWithId<SecureFileDownloadDefinition, Guid> Store { get; set; }

        /// <summary>
        /// Create a new <see cref="FlatFileEventSinkKnownEventDefinitionsStorage"/> with the given file path.
        /// </summary>
        /// <param name="filepath">Filepath to where the data will be stored.</param>
        public FlatFileSecureFileDownloadDefinitionStorage(string filepath)
        {
            Store = new HCSimpleDataStoreWithId<SecureFileDownloadDefinition, Guid>(
                filepath,
                serializer: new Func<SecureFileDownloadDefinition, string>((e) => JsonConvert.SerializeObject(e)),
                deserializer: new Func<string, SecureFileDownloadDefinition>((row) => JsonConvert.DeserializeObject<SecureFileDownloadDefinition>(row)),
                idSelector: (e) => e.Id,
                idSetter: (e, id) => e.Id = id,
                nextIdFactory: (events, e) => Guid.NewGuid()
            );
        }

        /// <summary>
        /// Create a new definition.
        /// </summary>
        public SecureFileDownloadDefinition CreateDefinition(SecureFileDownloadDefinition definition)
            => Store.InsertItem(definition);

        /// <summary>
        /// Get all stored definitions.
        /// </summary>
        public IEnumerable<SecureFileDownloadDefinition> GetDefinitions() => Store.GetEnumerable();

        /// <summary>
        /// Delete a stored definitions.
        /// </summary>
        public void DeleteDefinition(Guid id) => Store.DeleteItem(id);

        /// <summary>
        /// Retrieve a stored definition by id.
        /// </summary>
        public SecureFileDownloadDefinition GetDefinition(Guid id) => Store.GetItem(id);

        /// <summary>
        /// Retrieve a stored definition by <see cref="SecureFileDownloadDefinition.UrlSegmentText"/>.
        /// <para>Should be a case-insensitive check.</para>
        /// </summary>
        public SecureFileDownloadDefinition GetDefinitionByUrlSegmentText(string urlSegmentText)
            => Store.GetEnumerable().FirstOrDefault(x => x.UrlSegmentText?.Trim()?.ToLower() == urlSegmentText.Trim().ToLower());

        /// <summary>
        /// Edit an already stored definition.
        /// </summary>
        public SecureFileDownloadDefinition UpdateDefinition(SecureFileDownloadDefinition definition)
            => Store.InsertOrUpdateItem(definition);
    }
}
