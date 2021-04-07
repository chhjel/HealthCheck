using HealthCheck.Core.Modules.Metrics.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Core.Modules.Metrics.Context
{
    /// <summary>
    /// Contains the currently tracked metrics for this context.
    /// </summary>
    public class HCMetricsContext : IDisposable
    {
        /// <summary>
        /// Unique id.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// When the request started.
        /// </summary>
        public DateTime RequestTimestamp { get; }

        /// <summary>
        /// Any tracked timings and notes.
        /// </summary>
        public List<HCMetricsItem> Items { get; private set; } = new List<HCMetricsItem>();

        private readonly object _itemsLock = new();
        private bool _disposed = false;

        /// <summary>
        /// Contains the currently tracked metrics for this context.
        /// </summary>
        public HCMetricsContext(DateTime requestTimestamp)
        {
            RequestTimestamp = requestTimestamp;
        }

        /// <summary>
        /// Finalizer for <see cref="HCMetricsContext"/>.
        /// </summary>
        ~HCMetricsContext()
        {
            Dispose(false);
        }

        /// <summary>
        /// Ends and reports any tracked metrics.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Ends and reports any tracked metrics.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            EndAllTimingsInternal();

            var notify = false;
            lock (_itemsLock)
            {
                notify = Items.Any();
            }

            if (notify)
            {
                HCMetricsUtil.NotifyNewTrackedMetrics(this);
            }
        }

        /// <summary></summary>
        public override string ToString() => Id.ToString();

#pragma warning disable S4136 // Method overloads should be grouped together
        /// <summary>
        /// Add a note of what just happened without any duration data.
        /// </summary>
        public static void AddNote(string description)
            => WithCurrentContext((c) => c.AddNoteInternal(description));
        internal void AddNoteInternal(string description)
        {
            lock (_itemsLock)
            {
                Items.Add(HCMetricsItem.CreateNote(description, CreateOffset()));
            }
        }

        /// <summary>
        /// Add timing data with a given duration.
        /// </summary>
        public static void AddTiming(string description, TimeSpan duration)
            => WithCurrentContext((c) => c.AddTimingInternal(description, duration));
        internal void AddTimingInternal(string description, TimeSpan duration)
        {
            lock (_itemsLock)
            {
                Items.Add(HCMetricsItem.CreateTiming(null, description, DateTime.Now - RequestTimestamp - duration, duration));
            }
        }

        /// <summary>
        /// Start timing data with a given id.
        /// </summary>
        public static void StartTiming(string id, string description)
            => WithCurrentContext((c) => c.StartTimingInternal(id, description));
        internal void StartTimingInternal(string id, string description)
        {
            lock (_itemsLock)
            {
                Items.Add(HCMetricsItem.CreateTimingStart(id, description, CreateOffset()));
            }
        }

        /// <summary>
        /// Start timing data.
        /// </summary>
        public static void StartTiming(string description)
            => WithCurrentContext((c) => c.StartTimingInternal(description));
        internal void StartTimingInternal(string description)
        {
            lock (_itemsLock)
            {
                Items.Add(HCMetricsItem.CreateTimingStart(null, description, CreateOffset()));
            }
        }

        /// <summary>
        /// End a timing, if given id is null the latest timing will be stopped.
        /// </summary>
        /// <param name="id">Id of timing to stop, if null the latest timing will be stopped.</param>
        public static void EndTiming(string id = null)
            => WithCurrentContext((c) => c.EndTimingInternal(id));
        internal void EndTimingInternal(string id = null)
        {
            lock (_itemsLock)
            {
                var timing = Items.LastOrDefault(x => id == null || x.Id == id);
                timing?.EndTimer();
            }
        }

        /// <summary>
        /// Ends all Items.
        /// </summary>
        public static void EndAllTimings()
            => WithCurrentContext((c) => c.EndAllTimingsInternal());
        internal void EndAllTimingsInternal()
        {
            lock (_itemsLock)
            {
                foreach (var item in Items)
                {
                    item.EndTimer();
                }
            }
        }
#pragma warning restore S4136 // Method overloads should be grouped together

        private TimeSpan CreateOffset() => DateTime.Now - RequestTimestamp;

        internal static void WithCurrentContext(Action<HCMetricsContext> action)
        {
            try
            {
                var context = HCMetricsUtil.CurrentContextFactory?.Invoke();
                if (context == null)
                {
                    return;
                }

                action(context);
            }
            catch (Exception)
            {
                /* ignored */
            }
        }
    }
}
