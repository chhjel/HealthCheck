using HealthCheck.Core.Modules.AccessManager.Abstractions;
using HealthCheck.Core.Modules.AccessManager.Models;
using HealthCheck.Core.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace HealthCheck.WebUI.Services
{
    /// <summary>
    /// Stores and retrieves <see cref="HCAccessToken"/>s.
    /// </summary>
    public class FlatFileAccessManagerTokenStorage : IAccessManagerTokenStorage
    {
        private SimpleDataStoreWithId<HCAccessToken, Guid> Store { get; set; }

        /// <summary>
        /// Create a new <see cref="FlatFileAccessManagerTokenStorage"/> with the given file path.
        /// </summary>
        /// <param name="filepath">Filepath to where the data will be stored.</param>
        public FlatFileAccessManagerTokenStorage(string filepath)
        {
            Store = new SimpleDataStoreWithId<HCAccessToken, Guid>(
                filepath,
                serializer: new Func<HCAccessToken, string>((e) => JsonConvert.SerializeObject(e)),
                deserializer: new Func<string, HCAccessToken>((row) => JsonConvert.DeserializeObject<HCAccessToken>(row)),
                idSelector: (e) => e.Id,
                idSetter: (e, id) => e.Id = id,
                nextIdFactory: (events, e) => (e.Id == Guid.Empty ? Guid.NewGuid() : e.Id)
            );
        }

        /// <summary>
        /// Get all tokens.
        /// </summary>
        public IEnumerable<HCAccessToken> GetTokens()
            => Store.GetEnumerable();

        /// <summary>
        /// Get a single token by id.
        /// </summary>
        public HCAccessToken GetToken(Guid id)
            => Store.GetItem(id);

        /// <summary>
        /// Save a new token.
        /// </summary>
        public HCAccessToken SaveNewToken(HCAccessToken token)
            => Store.InsertItem(token);

        /// <summary>
        /// Delete a token.
        /// </summary>
        public void DeleteConfig(Guid tokenId)
            => Store.DeleteItem(tokenId);
    }
}
