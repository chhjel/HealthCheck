using System;

namespace HealthCheck.Core.Modules.Metrics.Models
{
    /// <summary>
    /// Traced metrics data.
    /// </summary>
    public class HCMetricsItem
    {
        /// <summary>
        /// Type of metric item.
        /// </summary>
        public MetricItemType Type { get; set; }

        /// <summary>
        /// Id of the event.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// When it happened.
        /// </summary>
        public DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// Duration of the timing.
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// Offset of the timing from the start of the request.
        /// </summary>
        public TimeSpan? Offset { get; set; }

        /// <summary>
        /// Description of what happened.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Exception details if any.
        /// </summary>
        public string ExceptionDetails { get; set; }

        /// <summary>
        /// Value of something.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Offset of the timing in milliseconds from the start of the request.
        /// </summary>
        public long OffsetMilliseconds => (long)(Offset?.TotalMilliseconds ?? 0);

        /// <summary>
        /// Duration of the timing in milliseconds.
        /// </summary>
        public long DurationMilliseconds => (long)(Duration?.TotalMilliseconds ?? 0);

        /// <summary>
        /// Offset + duration.
        /// </summary>
        public long EndMilliseconds => OffsetMilliseconds + DurationMilliseconds;

        /// <summary>
        /// Include timing if any to globally tracked values.
        /// </summary>
        public bool AddTimingToGlobals { get; private set; }

        /// <summary>
        /// Include note if any to globally tracked notes.
        /// </summary>
        public bool AddNoteToGlobals { get; private set; }

        /// <summary>
        /// Type of item.
        /// </summary>
        public enum MetricItemType
        {
            /// <summary>Duration of something</summary>
            Timing = 0,
            /// <summary>A single text</summary>
            Note,
            /// <summary>A text and a value</summary>
            Value,
            /// <summary>An errormessage</summary>
            Error
        }

        #region Factory
        /// <summary>
        /// Create a new note to be added globally with the given id.
        /// </summary>
        public static HCMetricsItem CreateGlobalNote(string id, string note)
            => new(MetricItemType.Note, id, note, null) { AddNoteToGlobals = true };

        /// <summary>
        /// Create a new note.
        /// </summary>
        public static HCMetricsItem CreateNote(string note, TimeSpan? offset)
            => new(MetricItemType.Note, null, note, offset);

        /// <summary>
        /// Create a new error.
        /// </summary>
        public static HCMetricsItem CreateError(string error, TimeSpan? offset)
            => CreateError(error, null, offset);

        /// <summary>
        /// Create a new error.
        /// </summary>
        public static HCMetricsItem CreateError(string error, Exception exception, TimeSpan? offset)
            => new(MetricItemType.Error, null, error, offset) { ExceptionDetails = exception?.ToString() };

        /// <summary>
        /// Create a new value.
        /// </summary>
        public static HCMetricsItem CreateValue(string id, string description, int value, TimeSpan? offset)
            => new(MetricItemType.Value, id, description, offset) { Value = value };

        /// <summary>
        /// Create a new timing.
        /// </summary>
        public static HCMetricsItem CreateTimingStart(string id, string description, TimeSpan? offset, bool addToGlobals = false)
            => new(MetricItemType.Timing, id, description, offset) { AddTimingToGlobals = addToGlobals };

        /// <summary>
        /// Create a new timing.
        /// </summary>
        public static HCMetricsItem CreateTiming(string id, string description, TimeSpan? offset, TimeSpan duration, bool addToGlobals = false)
            => new(MetricItemType.Timing, id, description, offset) { Duration = duration, AddTimingToGlobals = addToGlobals };
        #endregion

        internal HCMetricsItem(MetricItemType type, string id, string description, TimeSpan? offset)
        {
            Type = type;
            Id = id ?? Guid.NewGuid().ToString();
            Description = description;
            Timestamp = DateTimeOffset.Now;
            Offset = offset;
        }

        internal void EndTimer()
        {
            if (Timestamp != null && Duration == null && Type == MetricItemType.Timing)
            {
                Duration = DateTimeOffset.Now - Timestamp;
            }
        }

        /// <summary>Summary of the timing.</summary>
        public override string ToString() => $"{Description}: {DurationMilliseconds}ms";
    }
}
