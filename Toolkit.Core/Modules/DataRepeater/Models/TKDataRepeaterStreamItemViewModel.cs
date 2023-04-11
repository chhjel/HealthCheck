using QoDL.Toolkit.Core.Attributes;
using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Models
{
    /// <summary>
    /// Viewmodel for <see cref="TKDataRepeaterStreamItemsPagedModel"/>
    /// </summary>
    public class TKDataRepeaterStreamItemViewModel
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
        [TKRtProperty(ForcedNullable = true)]
        public DateTimeOffset? LastRetriedAt { get; set; }

        /// <summary>
        /// When this item was stored.
        /// </summary>
        [TKRtProperty(ForcedNullable = true)]
        public DateTimeOffset? LastUpdatedAt { get; set; }

        /// <summary>
        /// Was the last retry successful?
        /// </summary>
        [TKRtProperty(ForcedNullable = true)]
        public bool? LastRetryWasSuccessful { get; set; }

        /// <summary>
        /// When this item was last performed an action on.
        /// </summary>
        [TKRtProperty(ForcedNullable = true)]
        public DateTimeOffset? LastActionAt { get; set; }

        /// <summary>
        /// Optional id of the serialized data.
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// Optional short summary of the serialized data, displayed in the list along <see cref="ItemId"/>.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// If false, the item can't be retried processed from the interface.
        /// </summary>
        public bool AllowRetry { get; set; }

        /// <summary>
        /// Any tags applied to the item.
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>
        /// When the first error was stored.
        /// </summary>
        [TKRtProperty(ForcedNullable = true)]
        public DateTimeOffset? FirstErrorAt { get; set; }

        /// <summary>
        /// When the latest error was stored.
        /// </summary>
        [TKRtProperty(ForcedNullable = true)]
        public DateTimeOffset? LastErrorAt { get; set; }

        /// <summary>
        /// First registered error if any.
        /// <para>Is automatically set from <see cref="Error"/></para>
        /// </summary>
        public string FirstError { get; set; }

        /// <summary>
        /// Latest error if any.
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Last retry/action results.
        /// </summary>
        public List<TKDataRepeaterSimpleLogEntry> Log { get; set; }

        /// <summary>
        /// The data serialized to a string.
        /// </summary>
        public string SerializedData { get; set; }

        /// <summary>
        /// First version of serialized data stored.
        /// </summary>
        public string FirstSerializedData { get; set; }

        /// <summary>
        /// Optional time when this item will be deleted after.
        /// </summary>
        [TKRtProperty(ForcedNullable = true)]
        public DateTimeOffset? ExpiresAt { get; set; }

        /// <summary>
        /// Optional forced status to show in the UI.
        /// </summary>
        [TKRtProperty(ForcedNullable = true)]
        public TKDataRepeaterStreamItemStatus? ForcedStatus { get; set; }

        /// <summary>
        /// What actions can be executed or not.
        /// </summary>
        public List<TKDataRepeaterStreamItemActionAllowedViewModel> ActionValidationResults { get; set; } = new();
    }
}
