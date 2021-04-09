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

        /// <summary>
        /// Any globally tracked counters.
        /// </summary>
        public Dictionary<string, long> GlobalCounters { get; private set; } = new();

        /// <summary>
        /// Any globally tracked values.
        /// </summary>
        public Dictionary<string, List<long>> GlobalValues { get; private set; } = new();

        private readonly object _itemsLock = new();
        private readonly object _globalCountersLock = new();
        private readonly object _globalValuesLock = new();
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
        /// Increments a global counter
        /// </summary>
        public static void IncrementGlobalCounter(string id, int amount)
            => WithCurrentContext((c) => c.IncrementGlobalValueCounterInternal(id, amount));
        internal void IncrementGlobalValueCounterInternal(string id, int amount)
        {
            lock (_globalCountersLock)
            {
                if (!GlobalCounters.ContainsKey(id))
                {
                    GlobalCounters[id] = 0;
                }
                GlobalCounters[id] += amount;
            }
        }

        /// <summary>
        /// Add a global value that will be min/max/avgeraged.
        /// </summary>
        public static void AddGlobalValue(string id, int value)
            => WithCurrentContext((c) => c.AddGlobalValueInternal(id, value));
        internal void AddGlobalValueInternal(string id, int value)
        {
            lock (_globalValuesLock)
            {
                if (!GlobalValues.ContainsKey(id))
                {
                    GlobalValues[id] = new();
                }
                GlobalValues[id].Add(value);
            }
        }

        /// <summary>
        /// Add an error.
        /// </summary>
        public static void AddError(string errorMessage)
            => WithCurrentContext((c) => c.AddErrorInternal(errorMessage));
        internal void AddErrorInternal(string errorMessage)
        {
            lock (_itemsLock)
            {
                Items.Add(HCMetricsItem.CreateError(errorMessage, CreateOffset()));
            }
        }

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
        /// Add a note of what just happened without along with a value.
        /// </summary>
        public static void AddNote(string id, string description, int value)
            => WithCurrentContext((c) => c.AddNoteInternal(id, description, value));

        /// <summary>
        /// Add a note of what just happened without along with a value.
        /// </summary>
        public static void AddNote(string description, int value)
            => WithCurrentContext((c) => c.AddNoteInternal(null, description, value));
        internal void AddNoteInternal(string id, string description, int value)
        {
            lock (_itemsLock)
            {
                Items.Add(HCMetricsItem.CreateValue(id, description, value, CreateOffset()));
            }
        }

        /// <summary>
        /// Add timing data with a given duration.
        /// </summary>
        public static void AddTiming(string id, string description, TimeSpan duration, bool addToGlobals = false)
            => WithCurrentContext((c) => c.AddTimingInternal(id, description, duration, addToGlobals));

        /// <summary>
        /// Add timing data with a given duration.
        /// </summary>
        public static void AddTiming(string description, TimeSpan duration)
            => WithCurrentContext((c) => c.AddTimingInternal(null, description, duration, addToGlobals: false));
        internal void AddTimingInternal(string id, string description, TimeSpan duration, bool addToGlobals)
        {
            lock (_itemsLock)
            {
                Items.Add(HCMetricsItem.CreateTiming(id, description, DateTime.Now - RequestTimestamp - duration, duration, addToGlobals));
            }
        }

        /// <summary>
        /// Start timing data with a given id.
        /// </summary>
        public static void StartTiming(string id, string description, bool addToGlobals = false)
            => WithCurrentContext((c) => c.StartTimingInternal(id, description, addToGlobals));
        internal void StartTimingInternal(string id, string description, bool addToGlobals)
        {
            lock (_itemsLock)
            {
                Items.Add(HCMetricsItem.CreateTimingStart(id, description, CreateOffset(), addToGlobals));
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
