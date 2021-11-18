using HealthCheck.Core.Config;
using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary>
    /// Default item model that can be stored in a <see cref="IHCDataRepeaterStream"/>.
    /// <typeparamref name="TData"/> must be serializable.
    /// </summary>
    public class HCDefaultDataRepeaterStreamItem<TData> : IHCDataRepeaterStreamItem
        where TData : class, new()
    {
        #region IHCDataRepeaterStreamItem Implementation
        /// <inheritdoc />
        public Guid Id { get; set; }

        /// <inheritdoc />
        public DateTimeOffset InsertedAt { get; set; }

        /// <inheritdoc />
        public DateTimeOffset LastRetriedAt { get; set; }

        /// <inheritdoc />
        public string ItemId { get; set; }

        /// <inheritdoc />
        public string SerializedData { get; set; }

        /// <inheritdoc />
        public bool AllowRetry { get; set; }

        /// <inheritdoc />
        public HashSet<string> Tags { get; set; }

        /// <inheritdoc />
        public string InitialError { get; set; }

        /// <inheritdoc />
        public List<string> RetryResults { get; set; }
        #endregion

        /// <summary>
        /// Deserialize <see cref="SerializedData"/> into <typeparamref name="TData"/>.
        /// </summary>
        public TData Data => HCGlobalConfig.Serializer.Deserialize<TData>(SerializedData);

        /// <summary>
        /// Create a new item from the given <typeparamref name="TData"/>.
        /// </summary>
        public static HCDefaultDataRepeaterStreamItem<TData> CreateFrom(TData data, string itemId,
            string initialError = null, IEnumerable<string> tags = null, bool allowRetry = true)
            => new()
            {
                Id = Guid.NewGuid(),
                ItemId = itemId,
                Tags = new HashSet<string>(tags ?? Enumerable.Empty<string>()),
                SerializedData = HCGlobalConfig.Serializer.Serialize(data),
                AllowRetry = allowRetry,
                InsertedAt = DateTimeOffset.Now,
                InitialError = initialError
            };
    }
}
