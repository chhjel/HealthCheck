using EPiServer.Framework.Blobs;
using HealthCheck.Core.Modules.AccessTokens.Abstractions;
using HealthCheck.Core.Modules.AccessTokens.Models;
using HealthCheck.Utility.Storage.Abstractions;
using HealthCheck.Episerver.Utils;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Episerver.Storage
{
    /// <summary>
    /// Stores data in blob storage.
    /// </summary>
    public class HCEpiserverBlobAccessTokenStorage: HCSingleBlobStorageBase<HCEpiserverBlobAccessTokenStorage.HCAccessTokenBlobData>, IAccessManagerTokenStorage
    {
        /// <summary>
        /// Container id used if not overridden.
        /// </summary>
        protected virtual Guid DefaultContainerId => Guid.Parse("b00e5a54-2ade-4237-bc1c-aa9c32f77a8d");

        /// <summary>
        /// Defaults to the default provider if null.
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Defaults to a hardcoded guid if null
        /// </summary>
        public Guid? ContainerId { get; set; }

        /// <summary>
        /// Shortcut to <c>ContainerId ?? DefaultContainerId</c>
        /// </summary>
        protected Guid ContainerIdWithFallback => ContainerId ?? DefaultContainerId;

        /// <inheritdoc />
        protected override string CacheKey => $"__hc_{ContainerIdWithFallback}";

        private readonly EpiserverBlobHelper<HCAccessTokenBlobData> _blobHelper;

        /// <summary>
        /// Stores data in blob storage.
        /// </summary>
        public HCEpiserverBlobAccessTokenStorage(IBlobFactory blobFactory, IMemoryCache cache)
            : base(cache)
        {
            _blobHelper = new EpiserverBlobHelper<HCAccessTokenBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
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

        /// <inheritdoc />
        protected override HCAccessTokenBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

        /// <inheritdoc />
        protected override void StoreBlobData(HCAccessTokenBlobData data) => _blobHelper.StoreBlobData(data);

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
