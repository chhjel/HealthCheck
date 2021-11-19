﻿using System;
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
        /// When this item was last retried.
        /// </summary>
        DateTimeOffset LastRetriedAt { get; set; }

        /// <summary>
        /// Was the last retry successful?
        /// </summary>
        bool LastRetryWasSuccessful { get; set; }

        /// <summary>
        /// When this item was last performed an action on.
        /// </summary>
        DateTimeOffset LastActionAt { get; set; }

        /// <summary>
        /// Was the last action successful?
        /// </summary>
        bool LastActionWasSuccessful { get; set; }

        /// <summary>
        /// Optional id of the serialized data.
        /// </summary>
        string ItemId { get; set; }

        /// <summary>
        /// If false, the item can't be retried processed from the interface.
        /// </summary>
        bool AllowRetry { get; set; }

        /// <summary>
        /// Any tags applied to the item.
        /// </summary>
        HashSet<string> Tags { get; set; }

        /// <summary>
        /// Optional initial error.
        /// </summary>
        string InitialError { get; set; }

        /// <summary>
        /// Last retry/action results.
        /// </summary>
        List<string> Log { get; set; }

        /// <summary>
        /// The data serialized to a string.
        /// </summary>
        string SerializedData { get; set; }
    }
}
