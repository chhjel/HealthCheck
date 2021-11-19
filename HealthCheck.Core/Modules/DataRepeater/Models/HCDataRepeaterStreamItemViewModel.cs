using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary>
    /// Viewmodel for <see cref="HCDataRepeaterStreamItemsPagedModel"/>
    /// </summary>
    public class HCDataRepeaterStreamItemViewModel
    {
        /// <summary>
        /// Unique id of this item.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// When this item was stored.
        /// </summary>
        public DateTimeOffset InsertedAt { get; set; }

        /// <summary>
        /// When this item was last retried.
        /// </summary>
        public DateTimeOffset LastRetriedAt { get; set; }

        /// <summary>
        /// Was the last retry successful?
        /// </summary>
        public bool LastRetryWasSuccessful { get; set; }

        /// <summary>
        /// When this item was last performed an action on.
        /// </summary>
        public DateTimeOffset LastActionAt { get; set; }

        /// <summary>
        /// Was the last action successful?
        /// </summary>
        public bool LastActionWasSuccessful { get; set; }

        /// <summary>
        /// Optional id of the serialized data.
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// If false, the item can't be retried processed from the interface.
        /// </summary>
        public bool AllowRetry { get; set; }

        /// <summary>
        /// Any tags applied to the item.
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>
        /// Optional initial error.
        /// </summary>
        public string InitialError { get; set; }

        /// <summary>
        /// Last retry/action results.
        /// </summary>
        public List<string> Log { get; set; }

        /// <summary>
        /// The data serialized to a string.
        /// </summary>
        public string SerializedData { get; set; }
    }
}
