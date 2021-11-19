using HealthCheck.Core.Config;
using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary>
    /// Default item model that can be stored in a <see cref="IHCDataRepeaterStream"/>.
    /// <typeparamref name="TData"/> must be serializable. Can be a string or complex object.
    /// </summary>
    public class HCDefaultDataRepeaterStreamItem<TData> : IHCDataRepeaterStreamItem
        where TData : class
    {
        #region IHCDataRepeaterStreamItem Implementation
        /// <inheritdoc />
        public Guid Id { get; set; }

        /// <inheritdoc />
        public DateTimeOffset InsertedAt { get; set; }

        /// <inheritdoc />
        public string ItemId { get; set; }

        /// <inheritdoc />
        public string SerializedData { get; set; }

        /// <inheritdoc />
        public bool AllowRetry { get; set; }

        /// <inheritdoc />
        public DateTimeOffset LastRetriedAt { get; set; }

        /// <inheritdoc />
        public bool LastRetryWasSuccessful { get; set; }

        /// <inheritdoc />
        public DateTimeOffset LastActionAt { get; set; }

        /// <inheritdoc />
        public bool LastActionWasSuccessful { get; set; }

        /// <inheritdoc />
        public HashSet<string> Tags { get; set; }

        /// <inheritdoc />
        public string InitialError { get; set; }

        /// <inheritdoc />
        public List<string> Log { get; set; }
        #endregion

        /// <summary>
        /// Deserialize <see cref="SerializedData"/> into <typeparamref name="TData"/>.
        /// If <typeparamref name="TData"/> is of type string it will be returned.
        /// </summary>
        public TData Data
        {
            get
            {
                if (typeof(TData) == typeof(string)) return SerializedData as TData;
                else return HCGlobalConfig.Serializer.Deserialize<TData>(SerializedData);
            }
        }

        /// <summary>
        /// Create a new item from the given <typeparamref name="TData"/>.
        /// <typeparamref name="TData"/> must be serializable. Can be a string or complex object.
        /// </summary>
        public static HCDefaultDataRepeaterStreamItem<TData> CreateFrom(TData data, string itemId,
            string initialError = null, IEnumerable<string> tags = null, bool allowRetry = true)
        {
            string serializedData = null;
            if (data != null)
            {
                serializedData = typeof(TData) == typeof(string)
                    ? data as string
                    : HCGlobalConfig.Serializer.Serialize(data, pretty: true);
            }

            return new()
            {
                Id = Guid.NewGuid(),
                ItemId = itemId,
                Tags = new HashSet<string>(tags ?? Enumerable.Empty<string>()),
                SerializedData = serializedData,
                AllowRetry = allowRetry,
                InsertedAt = DateTimeOffset.Now,
                InitialError = initialError
            };
        }
    }
}
