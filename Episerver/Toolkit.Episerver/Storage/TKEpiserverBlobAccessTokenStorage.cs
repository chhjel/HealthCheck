using EPiServer.Framework.Blobs;
using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Modules.AccessTokens.Abstractions;
using QoDL.Toolkit.Core.Modules.AccessTokens.Models;
using QoDL.Toolkit.Core.Util.Collections;
using QoDL.Toolkit.Core.Util.Storage;
using QoDL.Toolkit.Episerver.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Episerver.Storage
{
    /// <summary>
    /// Stores data in blob storage.
    /// </summary>
    public class TKEpiserverBlobAccessTokenStorage
        : TKSingleBlobStorageBase<TKEpiserverBlobAccessTokenStorage.TKAccessTokenBlobData>, IAccessManagerTokenStorage
    {
        /// <summary>
        /// Container id used if not overridden.
        /// </summary>
        protected virtual Guid DefaultContainerId => Guid.Parse("850e5a54-2ade-4237-bc1c-aa9c32f77a8d");

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
        protected override string CacheKey => $"__tk_{ContainerIdWithFallback}";

        private readonly TKEpiserverBlobHelper<TKAccessTokenBlobData> _blobHelper;
        private readonly TKDelayedBufferQueue<AuditTimestampBufferQueueItem> _timestampBufferQueue;

        /// <summary>
        /// Stores data in blob storage.
        /// </summary>
        public TKEpiserverBlobAccessTokenStorage(IBlobFactory blobFactory, ITKCache cache)
            : base(cache)
        {
            _blobHelper = new TKEpiserverBlobHelper<TKAccessTokenBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
            _timestampBufferQueue = new TKDelayedBufferQueue<AuditTimestampBufferQueueItem>(OnTimestampBufferQueueCallback, TimeSpan.FromSeconds(10));
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
        public TKAccessToken GetToken(Guid id)
        {
            var data = GetBlobData();
            if (data?.AccessTokens != null && data.AccessTokens.TryGetValue(id, out var tokenData))
            {
                return tokenData;
            }
            return null;
        }

        /// <inheritdoc />
        public IEnumerable<TKAccessToken> GetTokens()
            => GetBlobData()?.AccessTokens?.Select(x => x.Value) ?? Enumerable.Empty<TKAccessToken>();

        /// <inheritdoc />
        public TKAccessToken SaveNewToken(TKAccessToken token)
        {
            var data = GetBlobData();
            data.AccessTokens[token.Id] = token;
            SaveBlobData(data);
            return token;
        }

        /// <inheritdoc />
        public TKAccessToken UpdateTokenLastUsedAtTime(Guid id, DateTimeOffset time)
        {
            _timestampBufferQueue.Add(new AuditTimestampBufferQueueItem { Id = id, Timestamp = time });
            return null;
        }

        /// <inheritdoc />
        protected override TKAccessTokenBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

        /// <inheritdoc />
        protected override void StoreBlobData(TKAccessTokenBlobData data) => _blobHelper.StoreBlobData(data);

        private void OnTimestampBufferQueueCallback(Queue<AuditTimestampBufferQueueItem> items)
        {
            if (items?.Any() != true)
            {
                return;
            }

            var data = GetBlobData();

            foreach (var item in items)
            {
                if (data?.AccessTokens != null && data.AccessTokens.TryGetValue(item.Id, out var token))
                {
                    token.LastUsedAt = item.Timestamp;
                }
            }

            SaveBlobData(data);
        }

        /// <summary>
        /// Model stored in blob storage.
        /// </summary>
        public class TKAccessTokenBlobData
        {
            /// <summary>
            /// All generated tokens by id.
            /// </summary>
            public Dictionary<Guid, TKAccessToken> AccessTokens { get; set; } = new Dictionary<Guid, TKAccessToken>();
        }

        private struct AuditTimestampBufferQueueItem
        {
            public Guid Id;
            public DateTimeOffset Timestamp;
        }
    }
}
