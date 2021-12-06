using HealthCheck.Core.Config;
using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

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
        public DateTimeOffset? LastUpdatedAt { get; set; }

        /// <inheritdoc />
        public string ItemId { get; set; }

        /// <inheritdoc />
        public HCDataRepeaterStreamItemStatus? ForcedStatus { get; set; }

        /// <inheritdoc />
        public string Summary { get; set; }

        /// <inheritdoc />
        public string SerializedData { get; set; }

        /// <inheritdoc />
        public string FirstSerializedData { get; set; }

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
        public DateTimeOffset? FirstErrorAt { get; set; }

        /// <inheritdoc />
        public DateTimeOffset? LastErrorAt { get; set; }

        /// <inheritdoc />
        public string FirstError { get; set; }

        /// <inheritdoc />
        public string Error { get; set; }

        /// <inheritdoc />
        public List<HCDataRepeaterSimpleLogEntry> Log { get; set; }
        #endregion

        /// <summary>
        /// Deserialize <see cref="SerializedData"/> into <typeparamref name="TData"/> on get and updates <see cref="SerializedData"/> when set.
        /// If <typeparamref name="TData"/> is of type string it will be returned.
        /// </summary>
        [IgnoreDataMember]
        public TData Data
        {
            get
            {
                var serializedData = SerializedData;
                if (typeof(TData) == typeof(string)) return serializedData as TData;
                else return HCGlobalConfig.Serializer.Deserialize<TData>(serializedData);
            }
            set
            {
                SerializedData = null;
                if (value != null)
                {
                    SerializedData = typeof(TData) == typeof(string)
                        ? value as string
                        : HCGlobalConfig.Serializer.Serialize(value, pretty: true);
                }
            }
        }

        /// <summary>
        /// Deserialize <see cref="FirstSerializedData"/> into <typeparamref name="TData"/>.
        /// If <typeparamref name="TData"/> is of type string it will be returned.
        /// </summary>
        [IgnoreDataMember]
        public TData OriginalData
        {
            get
            {
                if (typeof(TData) == typeof(string)) return FirstSerializedData as TData;
                else return HCGlobalConfig.Serializer.Deserialize<TData>(FirstSerializedData);
            }
        }

        /// <summary>
        /// Create a new item from the given <typeparamref name="TData"/>.
        /// <typeparamref name="TData"/> must be serializable. Can be a string or complex object.
        /// </summary>
        public static TSelf CreateFrom(TData data, string itemId, string summary = null,
            IEnumerable<string> tags = null, bool allowRetry = true,
            DateTimeOffset? expirationTime = null,
            string error = null, Exception exception = null)
        {
            string serializedData = null;
            if (data != null)
            {
                serializedData = typeof(TData) == typeof(string)
                    ? data as string
                    : HCGlobalConfig.Serializer.Serialize(data, pretty: true);
            }

            if (exception != null)
            {
                if (error != null)
                {
                    error += "\n\n";
                }
                else
                {
                    error = "";
                }
                error += ExceptionUtils.GetFullExceptionDetails(exception);
            }

            return new()
            {
                Id = Guid.NewGuid(),
                ItemId = itemId,
                Summary = summary,
                Tags = new HashSet<string>(tags ?? Enumerable.Empty<string>()),
                SerializedData = serializedData,
                FirstSerializedData = serializedData,
                AllowRetry = allowRetry,
                InsertedAt = DateTimeOffset.Now,
                FirstError = error,
                Error = error,
                FirstErrorAt = string.IsNullOrWhiteSpace(error) ? null : DateTimeOffset.Now,
                LastErrorAt = string.IsNullOrWhiteSpace(error) ? null : DateTimeOffset.Now,
                ExpirationTime = expirationTime,
                Log = new()
            };
        }

        /// <summary>
        /// Sets <see cref="Error"/> to the given message and optionally include exception details.
        /// <para>If <see cref="FirstError"/> is empty it will be updated as well.</para>
        /// </summary>
        public TSelf SetError(string error, Exception exception = null)
        {
            Error = error;
            if (exception != null)
            {
                Error += $"\n\n{ExceptionUtils.GetFullExceptionDetails(exception)}";
            }
            LastErrorAt = DateTimeOffset.Now;

            if (string.IsNullOrWhiteSpace(FirstError))
            {
                FirstError = Error;
                FirstErrorAt = DateTimeOffset.Now;
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

        /// <summary>
        /// Removes a tag from the item.
        /// </summary>
        public TSelf RemoveTag(string tag)
        {
            Tags.Remove(tag);
            return this as TSelf;
        }

        /// <summary>
        /// Removes tags from the item.
        /// </summary>
        public TSelf RemoveTags(params string[] tags)
        {
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    Tags.Remove(tag);
                }
            }
            return this as TSelf;
        }

        /// <summary>
        /// Adds/removes a tag from the item depending on the given boolean value.
        /// </summary>
        public TSelf SetTag(string tag, bool included)
        {
            if (included)
            {
                AddTag(tag);
            }
            else
            {
                RemoveTag(tag);
            }
            return this as TSelf;
        }

        /// <summary>
        /// Sets forced item status to display in the UI.
        /// </summary>
        public TSelf SetForcedStatus(HCDataRepeaterStreamItemStatus? forcedStatus)
        {
            ForcedStatus = forcedStatus;
            return this as TSelf;
        }

        /// <summary>
        /// Clears forced item status to display in the UI.
        /// <para>Same as SetForcedStatus(null)</para>
        /// </summary>
        public TSelf ClearForcedStatus()
            => SetForcedStatus(null);
    }
}
