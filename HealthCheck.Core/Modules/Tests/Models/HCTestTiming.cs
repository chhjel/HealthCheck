﻿using System;

namespace HealthCheck.Core.Modules.Tests.Models
{
    /// <summary>
    /// Timing data for use with HC runtime tests.
    /// </summary>
    public class HCTestTiming
    {
        /// <summary>
        /// Description of what is being timed.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Duration of the timing.
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// Offset of the timing from the start of the test execution.
        /// </summary>
        public TimeSpan? Offset { get; set; }

        /// <summary>
        /// Offset of the timing in milliseconds from the start of the test execution.
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

        internal string Id { get; set; }
        internal DateTime? Timestamp { get; set; }

        /// <summary>
        /// Create a new timing.
        /// </summary>
        /// <param name="description">Description of what is being timed.</param>
        /// <param name="duration">Duration of the timing.</param>
        public HCTestTiming(string description, TimeSpan? duration, TimeSpan? offset)
        {
            Id = Guid.NewGuid().ToString();
            Description = description;
            Duration = duration;
            Offset = offset;
        }

        internal HCTestTiming(string id, string description, TimeSpan? offset)
        {
            Id = id;
            Description = description;
            Timestamp = DateTime.Now;
            Offset = offset;
        }

        internal void EndTimer()
        {
            if (Timestamp != null && Duration == null)
            {
                Duration = DateTime.Now - Timestamp;
            }
        }
    }
}
