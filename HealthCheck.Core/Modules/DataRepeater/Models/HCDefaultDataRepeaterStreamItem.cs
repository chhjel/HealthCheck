using HealthCheck.Core.Config;
using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Util;
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
        public DateTimeOffset? ExpirationTime { get; set; }

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
            IEnumerable<string> tags = null, bool allowRetry = true,
            DateTimeOffset? expirationTime = null,
            string initialError = null, Exception initialErrorException = null)
        {
            string serializedData = null;
            if (data != null)
            {
                serializedData = typeof(TData) == typeof(string)
                    ? data as string
                    : HCGlobalConfig.Serializer.Serialize(data, pretty: true);
            }

            if (initialErrorException != null)
            {
                if (initialError != null)
                {
                    initialError += "\n\n";
                }
                else
                {
                    initialError = "";
                }
                initialError += ExceptionUtils.GetFullExceptionDetails(initialErrorException);
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
                InitialError = initialError,
                ExpirationTime = expirationTime,
                Log = new()
            };
        }

        /// <summary>
        /// Sets <see cref="InitialError"/> to the given message and optionally include exception details.
        /// </summary>
        public TSelf SetInitialError(string error, Exception exception = null)
        {
            InitialError = error;
            if (exception != null)
            {
                InitialError += $"\n\n{ExceptionUtils.GetFullExceptionDetails(exception)}";
            }
            return this as TSelf;
        }

        /// <summary>
        /// Optional time when this item should be deleted after.
        /// <para>Only takes effect if a storage implementation with support for it is used.</para>
        /// <para>Set to null to not expire it.</para>
        /// </summary>
        public TSelf SetExpirationTime(DateTimeOffset? time)
        {
            ExpirationTime = time;
            return this as TSelf;
        }

        /// <summary>
        /// Adds a tag to the item.
        /// </summary>
        public TSelf AddTag(string tag)
        {
            Tags.Add(tag);
            return this as TSelf;
        }

        /// <summary>
        /// Adds tags to the item.
        /// </summary>
        public TSelf AddTags(params string[] tags)
        {
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    Tags.Add(tag);
                }
            }
            return this as TSelf;
        }
    }
}
