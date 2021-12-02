using HealthCheck.Core.Modules.DataRepeater.Models;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.DataRepeater.Abstractions
{
    /// <summary>
    /// An item that can be stored in a <see cref="IHCDataRepeaterStream"/>.
    /// </summary>
    public interface IHCDataRepeaterStreamItem
    {
        /// <summary>
        /// Unique id of this item.
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// When this item was stored.
        /// </summary>
        DateTimeOffset InsertedAt { get; set; }

        /// <summary>
        /// When this item was stored.
        /// </summary>
        DateTimeOffset? LastUpdatedAt { get; set; }

        /// <summary>
        /// When this item was last retried.
        /// </summary>
        DateTimeOffset? LastRetriedAt { get; set; }

        /// <summary>
        /// Was the last retry successful?
        /// </summary>
        bool? LastRetryWasSuccessful { get; set; }

        /// <summary>
        /// When this item was last performed an action on.
        /// </summary>
        DateTimeOffset? LastActionAt { get; set; }

        /// <summary>
        /// Optional time when this item should be deleted after.
        /// <para>Only takes effect if a storage implementation with support for it is used.</para>
        /// </summary>
        DateTimeOffset? ExpirationTime { get; set; }

        /// <summary>
        /// Optional id of the serialized data.
        /// </summary>
        string ItemId { get; set; }

        /// <summary>
        /// Optional forced status to show in the UI.
        /// </summary>
        HCDataRepeaterStreamItemStatus? ForcedStatus { get; set; }

        /// <summary>
        /// Optional short summary of the serialized data, displayed in the list along <see cref="ItemId"/>.
        /// </summary>
        string Summary { get; set; }

        /// <summary>
        /// If false, the item can't be retried processed from the interface.
        /// </summary>
        bool AllowRetry { get; set; }

        /// <summary>
        /// Any tags applied to the item.
        /// </summary>
        HashSet<string> Tags { get; set; }

        /// <summary>
        /// When the first error was stored.
        /// </summary>
        DateTimeOffset? FirstErrorAt { get; set; }

        /// <summary>
        /// When the latest error was stored.
        /// </summary>
        DateTimeOffset? LastErrorAt { get; set; }

        /// <summary>
        /// First registered error if any.
        /// <para>Is automatically set from <see cref="Error"/></para>
        /// </summary>
        string FirstError { get; set; }

        /// <summary>
        /// Latest error if any.
        /// </summary>
        string Error { get; set; }

        /// <summary>
        /// Last retry/action results.
        /// </summary>
        List<HCDataRepeaterSimpleLogEntry> Log { get; set; }

        /// <summary>
        /// The data serialized to a string.
        /// </summary>
        string SerializedData { get; set; }

        /// <summary>
        /// First version of the serialized data that was stored.
        /// </summary>
        string FirstSerializedData { get; set; }
    }
}
