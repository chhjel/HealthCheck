using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Config;
using System;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Util;

/// <summary>
/// Utility for checking self site uptime by storing the last uptime periodically and checking against it.
/// <para>Initialize on startup using <c>TKSelfUptimeChecker.EnsureIntervalCheckStarted(..)</c></para>
/// <para>Any exceptions will be sent to <see cref="TKGlobalConfig.OnExceptionEvent"/></para>
/// </summary>
public class TKSelfUptimeChecker
{
    private static TKSelfUptimeChecker Instance { get; } = new();

    /// <summary>
    /// Callback type for when a downtime was detected.
    /// </summary>
    public delegate void OnBackUpAfterDowntime(DateTimeOffset startedAt, DateTimeOffset endedAt, TimeSpan duration);

    private bool _checkStarted = false;
    private readonly object _checkStartedLock = new();
    private long _cancelCounter;

    private ITKSelfUptimeCheckerStorage _storage;
    private TimeSpan _failIfNoCheckForDuration;
    private OnBackUpAfterDowntime _onDowntimeDetected;

    /// <summary>
    /// For use during testing. Should not be needed used otherwise.
    /// </summary>
    public void Stop()
    {
        _checkStarted = false;
        _cancelCounter++;
    }

    /// <summary>
    /// Will only perform any action the first time executed unless <see cref="Stop"/> was invoked.
    /// </summary>
    /// <param name="storage">Where to store the last checked time.</param>
    /// <param name="checkInterval">How often to store a timestamp into <paramref name="storage"/></param>
    /// <param name="failIfNoCheckForDuration">If the duration between the last stored and the current time is greater than this, <paramref name="onDowntimeDetected"/> will be invoked.</param>
    /// <param name="onDowntimeDetected">Callback for when downtime was detected. Will be invoked after the site is back up.</param>
    public static void EnsureIntervalCheckStarted(ITKSelfUptimeCheckerStorage storage, TimeSpan checkInterval, TimeSpan failIfNoCheckForDuration, OnBackUpAfterDowntime onDowntimeDetected)
    {
        var instance = Instance;
        lock (instance._checkStartedLock)
        {
            if (instance._checkStarted) return;

            instance._storage = storage;
            instance._failIfNoCheckForDuration = failIfNoCheckForDuration;
            instance._onDowntimeDetected = onDowntimeDetected;
            Task.Run(() => instance.IntervalCheckAsync(checkInterval));

            instance._checkStarted = true;
        }
    }

    private async Task IntervalCheckAsync(TimeSpan checkInterval)
    {
        var cancelNumber = _cancelCounter;
        var cancel = false;
        while (!cancel)
        {
            cancel = cancelNumber != _cancelCounter;
            try
            {
                await Task.Delay(checkInterval);
                if (cancel) return;
                await CheckAsync();
            }
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKSelfUptimeChecker), nameof(IntervalCheckAsync), ex);
            }
        }
    }

    private async Task CheckAsync()
    {
        var now = DateTimeOffset.Now;
        var lastCheckedAt = await _storage.GetLastCheckedAtAsync();
        if (lastCheckedAt == null)
        {
            await _storage.StoreLastCheckedAtAsync(now);
            return;
        }

        await _storage.StoreLastCheckedAtAsync(now);

        var timeSinceLastCheck = now - lastCheckedAt.Value;
        if (timeSinceLastCheck >= _failIfNoCheckForDuration)
        {
            _onDowntimeDetected?.Invoke(lastCheckedAt.Value, now, timeSinceLastCheck);
        }
    }
}
