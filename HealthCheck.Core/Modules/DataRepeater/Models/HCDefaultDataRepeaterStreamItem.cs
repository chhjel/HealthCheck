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
    public class HCDefaultDataRepeaterStreamItem<TData, TSelf> : IHCDataRepeaterStreamItem
        where TData : class
        where TSelf : HCDefaultDataRepeaterStreamItem<TData, TSelf>, new()
    {
        #region IHCDataRepeaterStreamItem Implementation
        /// <inheritdoc />
        public Guid Id { get; set; }

        /// <inheritdoc />
        public DateTimeOffset InsertedAt { get; set; } = DateTimeOffset.Now;

        /// <inheritdoc />
        public string ItemId { get; set; }

        /// <inheritdoc />
        public string Summary { get; set; }

        /// <inheritdoc />
        public string SerializedData { get; set; }

        /// <inheritdoc />
        public string SerializedDataOverride { get; set; }

        /// <inheritdoc />
        public bool AllowRetry { get; set; }

        /// <inheritdoc />
        public DateTimeOffset? LastRetriedAt { get; set; }

        /// <inheritdoc />
        public bool? LastRetryWasSuccessful { get; set; }

        /// <inheritdoc />
        public DateTimeOffset? LastActionAt { get; set; }

        /// <inheritdoc />
        public HashSet<string> Tags { get; set; } = new();

        /// <inheritdoc />
        public string InitialError { get; set; }

        /// <inheritdoc />
        public List<HCDataRepeaterSimpleLogEntry> Log { get; set; }
        #endregion

        /// <summary>
        /// Deserialize <see cref="SerializedData"/> into <typeparamref name="TData"/>.
        /// <para>If <see cref="SerializedDataOverride"/> from frontend is not empty it will be used instead.</para>
        /// If <typeparamref name="TData"/> is of type string it will be returned.
        /// </summary>
        public TData Data
        {
            get
            {
                var serializedData = string.IsNullOrWhiteSpace(SerializedDataOverride) ? SerializedData : SerializedDataOverride;
                if (typeof(TData) == typeof(string)) return serializedData as TData;
                else return HCGlobalConfig.Serializer.Deserialize<TData>(serializedData);
            }
            set
            {
                SerializedDataOverride = null;
                if (value != null)
                {
                    SerializedDataOverride = typeof(TData) == typeof(string)
                        ? value as string
                        : HCGlobalConfig.Serializer.Serialize(value, pretty: true);
                }
            }
        }

        /// <summary>
        /// Deserialize <see cref="SerializedData"/> into <typeparamref name="TData"/>.
        /// If <typeparamref name="TData"/> is of type string it will be returned.
        /// </summary>
        public TData OriginalData
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
        public static TSelf CreateFrom(TData data, string itemId, string summary = null,
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
                Summary = summary,
                Tags = new HashSet<string>(tags ?? Enumerable.Empty<string>()),
                SerializedData = serializedData,
                AllowRetry = allowRetry,
                InsertedAt = DateTimeOffset.Now,
                InitialError = initialError
            };
        }
    }
}
