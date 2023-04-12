using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Core.Modules.EventNotifications.Models;

/// <summary>
/// A model for event sink notification configs.
/// </summary>
public class EventSinkNotificationConfig
{
    /// <summary>
    /// Id of this config instance.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of user that last changed this notifier.
    /// </summary>
    public string LastChangedBy { get; set; }

    /// <summary>
    /// Date when last changed.
    /// </summary>
    public DateTimeOffset LastChangedAt { get; set; }

    /// <summary>
    /// Date of latest notification.
    /// </summary>
    public DateTimeOffset? LastNotifiedAt { get; set; }

    /// <summary>
    /// Enable this config.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Optionally limit number of notifications before disabling it.
    /// </summary>
    public int? NotificationCountLimit { get; set; }

    /// <summary>
    /// Optionally limit notifications by this distinct key that supports placeholders.
    /// </summary>
    public string DistinctNotificationKey { get; set; }

    /// <summary>
    /// If <see cref="DistinctNotificationKey"/> is provided, this is how long distinct values are cached for.
    /// </summary>
    public TimeSpan DistinctNotificationCacheDuration { get; set; }

    /// <summary>
    /// Optionally limit notifications to after this time.
    /// </summary>
    public DateTimeOffset? FromTime { get; set; }

    /// <summary>
    /// Optionally limit notifications to before this time.
    /// </summary>
    public DateTimeOffset? ToTime { get; set; }

    /// <summary>
    /// Id of notifiers to notify when events match.
    /// </summary>
    public List<NotifierConfig> NotifierConfigs { get; set; }

    /// <summary>
    /// Filter for event event ids.
    /// </summary>
    public EventSinkNotificationConfigFilter EventIdFilter { get; set; }

    /// <summary>
    /// Filter for event payloads.
    /// </summary>
    public List<EventSinkNotificationConfigFilter> PayloadFilters { get; set; }

    /// <summary>
    /// 10 last notification results.
    /// </summary>
    public List<string> LatestResults { get; set; } = new List<string>();

    private static readonly TKSimpleMemoryCache<DateTimeOffset?> _distinctCache = new();
    private bool DistinctCacheEnabled => !string.IsNullOrWhiteSpace(DistinctNotificationKey) && DistinctNotificationCacheDuration.TotalSeconds > 0;

    /// <summary>
    /// Returns true if all <see cref="PayloadFilters"/> match the input.
    /// </summary>
    public bool IsAllowed(string eventId, bool payloadIsComplex,
        string stringifiedPayload, Dictionary<string, string> payloadProperties)
    {
        lock (_distinctCache)
        {
            _distinctCache.RemoveExpired(TimeSpan.FromSeconds(1));

            if (LimitHasBeenReached())
            {
                return false;
            }
            else if (FromTime != null && DateTimeOffset.Now.ToUniversalTime() < FromTime?.ToUniversalTime())
            {
                return false;
            }
            else if (EventIdFilter?.IsAllowed(false, eventId, null) == false)
            {
                return false;
            }
            else if (HasDistinctLimitBeenReached(payloadProperties))
            {
                return false;
            }
            else if (PayloadFilters == null || !PayloadFilters.Any())
            {
                TryUpdateDistinctCache(payloadProperties);
                return true;
            }
            else if (PayloadFilters?.All(x => x.IsAllowed(payloadIsComplex, stringifiedPayload, payloadProperties)) != true)
            {
                return false;
            }

            TryUpdateDistinctCache(payloadProperties);
        }

        return true;

        void TryUpdateDistinctCache(Dictionary<string, string> payloadProperties)
        {
            var distinctCacheKey = ResolvePlaceholders(DistinctNotificationKey, payloadProperties);
            UpdateDistinctCacheIfEnabled(distinctCacheKey);
        }
    }
    
    /// <summary>
    /// To clear static caches during tests.
    /// </summary>
    internal void ClearCaches()
    {
        _distinctCache.ClearAll();
    }

    private void UpdateDistinctCacheIfEnabled(string key)
    {
        if (key == null || !DistinctCacheEnabled)
        {
            return;
        }

        _distinctCache.SetFlag(key, DistinctNotificationCacheDuration);
    }

    private bool HasDistinctLimitBeenReached(Dictionary<string, string> placeholders)
    {
        if (!DistinctCacheEnabled)
        {
            return false;
        }

        var distinctCacheKey = ResolvePlaceholders(DistinctNotificationKey, placeholders);
        return _distinctCache.HasFlag(distinctCacheKey);
    }

    private string ResolvePlaceholders(string value, Dictionary<string, string> placeholders)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        foreach (var kvp in placeholders)
        {
            var key = kvp.Key;
            var placeholderValue = kvp.Value ?? "";

            value = value.Replace($"{{{key?.ToUpper()}}}", placeholderValue);
        }

        return value;
    }


    /// <summary>
    /// True if any defined limits has been reached.
    /// </summary>
    public bool LimitHasBeenReached()
    {
        if (NotificationCountLimit != null && NotificationCountLimit <= 0)
        {
            return true;
        }
        else if (ToTime != null && DateTimeOffset.Now.ToUniversalTime() > ToTime?.ToUniversalTime())
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Summary.
    /// </summary>
    public override string ToString() => $"[{Id}] Enabled: {Enabled}, LastChangedBy: {LastChangedBy}";
}
