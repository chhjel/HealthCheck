using EPiServer.Framework.Blobs;
using HealthCheck.Core.Modules.AccessTokens.Abstractions;
using HealthCheck.Core.Modules.AccessTokens.Models;
using HealthCheck.Episerver.Storage.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Episerver.Storage
{
    /// <summary>
    /// Stores data in blob storage.
    /// </summary>
    public class HCEpiserverBlobAccessTokenStorage: HCEpiserverSingleBlobStorageBase<HCEpiserverBlobAccessTokenStorage.HCAccessTokenBlobData>, IAccessManagerTokenStorage
    {
        /// <inheritdoc />
        protected override Guid DefaultContainerId => Guid.Parse("b00e5a54-2ade-4237-bc1c-aa9c32f77a8d");

        /// <summary>
        /// Stores data in blob storage.
        /// </summary>
        public HCEpiserverBlobAccessTokenStorage(IBlobFactory blobFactory, IMemoryCache cache)
            : base(blobFactory, cache)
        {
        }

        /// <inheritdoc />
        public void DeleteToken(Guid tokenId)
        {
            var data = GetBlobData();
            if (data?.AccessTokens?.ContainsKey(tokenId) == true)
            {
                data.AccessTokens.Remove(tokenId);
                SaveBlobData(data);
            }
        }

        /// <inheritdoc />
        public HCAccessToken GetToken(Guid id)
        {
            var data = GetBlobData();
            if (data?.AccessTokens != null && data.AccessTokens.TryGetValue(id, out var tokenData))
            {
                return tokenData;
            }
            return null;
        }

        /// <inheritdoc />
        public IEnumerable<HCAccessToken> GetTokens()
            => GetBlobData()?.AccessTokens?.Select(x => x.Value) ?? Enumerable.Empty<HCAccessToken>();

        /// <inheritdoc />
        public HCAccessToken SaveNewToken(HCAccessToken token)
        {
            var data = GetBlobData();
            data.AccessTokens[token.Id] = token;
            SaveBlobData(data);
            return token;
        }

        /// <inheritdoc />
        public HCAccessToken UpdateTokenLastUsedAtTime(Guid id, DateTimeOffset time)
        {
            var token = GetToken(id);
            if (token != null)
            {
                token.LastUsedAt = time;
            }
            return token;
        }

        /// <summary>
        /// Model stored in blob storage.
        /// </summary>
        public class HCAccessTokenBlobData
        {
            /// <summary>
            /// All generated tokens by id.
            /// </summary>
            public Dictionary<Guid, HCAccessToken> AccessTokens { get; set; }
        }
    }
}
