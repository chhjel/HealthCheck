using QoDL.Toolkit.Core.Modules.Metrics.Models;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace QoDL.Toolkit.Core.Modules.Metrics.Context;

/// <summary>
/// Contains the currently tracked metrics for this context.
/// </summary>
public class TKMetricsContext : IDisposable
{
    /// <summary>
    /// Unique id.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// When the request started.
    /// </summary>
    public DateTimeOffset RequestTimestamp { get; }

    /// <summary>
    /// Any tracked timings and notes.
    /// </summary>
    public List<TKMetricsItem> Items { get; set; } = new List<TKMetricsItem>();

    /// <summary>
    /// Any globally tracked counters.
    /// </summary>
    public Dictionary<string, long> GlobalCounters { get; set; } = new();

    /// <summary>
    /// Any globally tracked values.
    /// </summary>
    public Dictionary<string, List<long>> GlobalValues { get; set; } = new();

    /// <summary>
    /// Optional suffixes for global values.
    /// </summary>
    public Dictionary<string, string> GlobalValueSuffixes { get; set; } = new();

    /// <summary>
    /// True if there's any data to display.
    /// </summary>
    public bool ContainsData => Items.Any() || GlobalCounters.Any() || GlobalValues.Any();

    private readonly object _itemsLock = new();
    private readonly object _globalCountersLock = new();
    private readonly object _globalValuesLock = new();
    private bool _disposed = false;

    /// <summary>
    /// Contains the currently tracked metrics for this context.
    /// </summary>
    public TKMetricsContext(DateTimeOffset requestTimestamp)
    {
        RequestTimestamp = requestTimestamp;
    }

    /// <summary>
    /// Contains the currently tracked metrics for this context.
    /// </summary>
    public TKMetricsContext() { }

    /// <summary>
    /// Finalizer for <see cref="TKMetricsContext"/>.
    /// </summary>
    ~TKMetricsContext()
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
            notify = ContainsData;
        }

        if (notify)
        {
            TKMetricsUtil.NotifyNewTrackedMetrics(this);
        }
    }

    /// <summary></summary>
    public override string ToString() => Id.ToString();

    #region Statics
    /// <summary>
    /// Increments a global counter by the given value.
    /// </summary>
    public static void IncrementGlobalCounter(string id, int amount = 1)
        => WithCurrentContext((c) => c.IncrementGlobalValueCounterInternal(id, amount));
    internal void IncrementGlobalValueCounterInternal(string id, int amount)
    {
        lock (_globalCountersLock)
        {
            if (!GlobalCounters.ContainsKey(id))
            {
                GlobalCounters[id] = 0;
            }
            if (GlobalCounters[id] < long.MaxValue)
            {
                GlobalCounters[id] += amount;
            }
        }
    }

    /// <summary>
    /// Add a global value that will be min/max/avgeraged.
    /// </summary>
    public static void AddGlobalValue(string id, int value, string suffix = null)
        => WithCurrentContext((c) => c.AddGlobalValueInternal(id, value, suffix));

    /// <summary>
    /// Add a global value that will be min/max/avgeraged.
    /// </summary>
    public static void AddGlobalValue(string id, long value, string suffix = null)
        => WithCurrentContext((c) => c.AddGlobalValueInternal(id, value, suffix));

    internal void AddGlobalValueInternal(string id, long value, string suffix = null)
    {
        lock (_globalValuesLock)
        {
            if (!GlobalValues.ContainsKey(id))
            {
                GlobalValues[id] = new();
            }
            GlobalValues[id].Add(value);

            if (!string.IsNullOrWhiteSpace(suffix))
            {
                GlobalValueSuffixes[id] = suffix;
            }
        }
    }
    
    /// <summary>
    /// Add a timer to global timing values.
    /// </summary>
    public static void AddGlobalTimingValue(TKMetricsTimer timer)
    {
        if (timer == null) return;
        WithCurrentContext((c) => c.AddGlobalTimingValueInternal(timer));
    }
    internal void AddGlobalTimingValueInternal(TKMetricsTimer timer)
        => AddGlobalValueInternal(timer.Id, timer.Stopwatch.ElapsedMilliseconds, _timingValueSuffix);

    /// <summary>
    /// Time an action and add to global values.
    /// </summary>
    public static void AddGlobalTimingValue(string id, Action actionToTime)
        => WithCurrentContext((c) => c.AddGlobalTimingValueInternal(id, actionToTime));
    internal void AddGlobalTimingValueInternal(string id, Action actionToTime)
    {
        var watch = new Stopwatch();
        watch.Start();

        actionToTime?.Invoke();

        var duration = watch.ElapsedMilliseconds;
        AddGlobalValueInternal(id, duration, _timingValueSuffix);
    }
    private const string _timingValueSuffix = " ms";

    /// <summary>
    /// Time an action and add to global values.
    /// </summary>
    public static T AddGlobalTimingValue<T>(string id, Func<T> actionToTime)
        => WithCurrentContext((c) =>
        {
            if (c != null)
            {
                return c.AddGlobalTimingValueInternal(id, actionToTime);
            }
            else if (actionToTime != null)
            {
                return actionToTime();
            }
            return default;
        });

    internal T AddGlobalTimingValueInternal<T>(string id, Func<T> actionToTime)
    {
        var watch = new Stopwatch();
        watch.Start();
        
        var value = actionToTime();

        var duration = watch.ElapsedMilliseconds;
        AddGlobalValueInternal(id, duration);

        return value;
    }

    /// <summary>
    /// Add an error.
    /// <para>Stores no data. Only used to display in current request using <see cref="TKMetricsUtil.CreateContextSummaryHtml"/>.</para>
    /// </summary>
    public static void AddError(string errorMessage, Exception exception = null)
        => WithCurrentContext((c) => c.AddErrorInternal(errorMessage, exception));
    internal void AddErrorInternal(string errorMessage, Exception exception = null)
    {
        lock (_itemsLock)
        {
            Items.Add(TKMetricsItem.CreateError(errorMessage, exception, CreateOffset()));
        }
    }

    /// <summary>
    /// Add a note with a given key to store globally. Overwrites any existing note with the same key.
    /// </summary>
    public static void AddGlobalNote(string id, string description)
        => WithCurrentContext((c) => c.AddGlobalNoteInternal(id, description));
    internal void AddGlobalNoteInternal(string id, string description)
    {
        lock (_itemsLock)
        {
            Items.Add(TKMetricsItem.CreateGlobalNote(id, description));
        }
    }

    /// <summary>
    /// Add a note of what just happened without any duration data.
    /// <para>Stores no data. Only used to display in current request using <see cref="TKMetricsUtil.CreateContextSummaryHtml"/>.</para>
    /// </summary>
    public static void AddNote(string description)
        => WithCurrentContext((c) => c.AddNoteInternal(description));
    internal void AddNoteInternal(string description)
    {
        lock (_itemsLock)
        {
            Items.Add(TKMetricsItem.CreateNote(description, CreateOffset()));
        }
    }

    /// <summary>
    /// Add a note of what just happened without along with a value.
    /// <para>Stores no data. Only used to display in current request using <see cref="TKMetricsUtil.CreateContextSummaryHtml"/>.</para>
    /// </summary>
    public static void AddNote(string id, string description, int value)
        => WithCurrentContext((c) => c.AddNoteInternal(id, description, value));

    /// <summary>
    /// Add a note of what just happened without along with a value.
    /// <para>Stores no data. Only used to display in current request using <see cref="TKMetricsUtil.CreateContextSummaryHtml"/>.</para>
    /// </summary>
    public static void AddNote(string description, int value)
        => WithCurrentContext((c) => c.AddNoteInternal(null, description, value));
    internal void AddNoteInternal(string id, string description, int value)
    {
        lock (_itemsLock)
        {
            Items.Add(TKMetricsItem.CreateValue(id, description, value, CreateOffset()));
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
            Items.Add(TKMetricsItem.CreateTiming(id, description, DateTimeOffset.Now - RequestTimestamp - duration, duration, addToGlobals));
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
            Items.Add(TKMetricsItem.CreateTimingStart(id, description, CreateOffset(), addToGlobals));
        }
    }

    /// <summary>
    /// Start timing data.
    /// <para>Stores no data. Only used to display in current request using <see cref="TKMetricsUtil.CreateContextSummaryHtml"/>.</para>
    /// </summary>
    public static void StartTiming(string description)
        => WithCurrentContext((c) => c.StartTimingInternal(description));
    internal void StartTimingInternal(string description)
    {
        lock (_itemsLock)
        {
            Items.Add(TKMetricsItem.CreateTimingStart(null, description, CreateOffset()));
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

    /// <summary>
    /// Callback from <see cref="ExecuteIfTimingIsSlowerThan"/>.
    /// </summary>
    /// <param name="actualDuration">Actual duration the timing used.</param>
    /// <param name="actualDurationText">Actual duration the timing used in prettified string format.</param>
    public delegate void TimingSlowAction(TimeSpan actualDuration, string actualDurationText);

    /// <summary>
    /// Executes the given action if a timing with the given id has a longer duration than the given duration.
    /// </summary>
    /// <param name="id">Id of action to check. If null, the last timing will be used.</param>
    /// <param name="duration">Duration threshold.</param>
    /// <param name="action">Callback to invoke if duration threshold was breached.</param>
    public static void ExecuteIfTimingIsSlowerThan(string id, TimeSpan duration, TimingSlowAction action)
    {
        WithCurrentContext(c =>
        {
            var timing = c.Items.LastOrDefault(x => x.Type == TKMetricsItem.MetricItemType.Timing && (x.Id == id || id == null));
            var timingDuration = timing.Duration ?? (DateTimeOffset.Now - (timing.Timestamp ?? DateTimeOffset.Now));
            if (timingDuration > duration)
            {
                action(timingDuration, TKTimeUtils.PrettifyDuration(timingDuration));
            }
        });
    }
    #endregion

    private TimeSpan CreateOffset() => DateTimeOffset.Now - RequestTimestamp;

    internal static void WithCurrentContext(Action<TKMetricsContext> action)
    {
        try
        {
            var context = TKMetricsUtil.CurrentContextFactory?.Invoke();
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

    internal static T WithCurrentContext<T>(Func<TKMetricsContext, T> action)
    {
        try
        {
            var context = TKMetricsUtil.CurrentContextFactory?.Invoke();
            return action(context);
        }
        catch (Exception)
        {
            /* ignored */
        }
        return default;
    }
}
