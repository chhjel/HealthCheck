using EPiServer.Data.Dynamic;
using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Episerver.Models;
using QoDL.Toolkit.Episerver.Storage;
using System;
using System.Linq;

namespace QoDL.Toolkit.Episerver.Utils;

/// <summary>
/// Various episerver utilities.
/// </summary>
public static class TKEpiserverUtils
{
    /// <summary>
    /// Checks if the given version is different from one stored in epi DDS.
    /// <para>If the version differs the new version will be written to DDS and the given action invoked.</para>
    /// <para>Use to execute things once per deployed version.</para>
    /// <para>DDS model type: <see cref="TKLastCheckedVersionData"/></para>
    /// </summary>
    /// <param name="currentVersion">Current version to check against.</param>
    /// <param name="actionOnNewVersion">Action to execute if a new version was found. Parameter is the value of <paramref name="currentVersion"/>.</param>
    public static void ExecuteIfNewlyDeployedVersion(string currentVersion, Action<string> actionOnNewVersion)
    {
        try
        {
            var store = typeof(TKLastCheckedVersionData).GetOrCreateStore();
            var versionData = store.Items<TKLastCheckedVersionData>().FirstOrDefault();

            if (versionData == null)
            {
                var data = new TKLastCheckedVersionData { LastCheckedVersion = currentVersion };
                store.Save(data);
                actionOnNewVersion?.Invoke(currentVersion);
            }
            else if (versionData.LastCheckedVersion != currentVersion)
            {
                versionData.LastCheckedVersion = currentVersion;
                store.Save(versionData);
                actionOnNewVersion?.Invoke(currentVersion);
            }
        }
        catch (Exception ex)
        {
            TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKEpiserverUtils), nameof(ExecuteIfNewlyDeployedVersion), ex);
        }
    }

    /// <summary>
    /// Shortcut to <see cref="TKSelfUptimeChecker.EnsureIntervalCheckStarted"/> using a storage implementation using epi DDS.
    /// <para>Storage implementation used: <see cref="TKEpiserverDDSSelfUptimeCheckerStorage"/>.</para>
    /// </summary>
		/// <param name="checkInterval">How often to store a timestamp into storage. E.g. 2 minutes.</param>
		/// <param name="failIfNoCheckForDuration">If the duration between the last stored and the current time is greater than this, <paramref name="onDowntimeDetected"/> will be invoked. E.g. 5 minutes.</param>
		/// <param name="onDowntimeDetected">Callback for when downtime was detected. Will be invoked after the site is back up.</param>
    public static void StartSelfUptimeChecker(TimeSpan checkInterval, TimeSpan failIfNoCheckForDuration, TKSelfUptimeChecker.OnBackUpAfterDowntime onDowntimeDetected)
    {
        try
        {
            var storage = new TKEpiserverDDSSelfUptimeCheckerStorage();
            TKSelfUptimeChecker.EnsureIntervalCheckStarted(storage, checkInterval, failIfNoCheckForDuration, onDowntimeDetected);
        }
        catch (Exception ex)
        {
            TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKEpiserverUtils), nameof(StartSelfUptimeChecker), ex);
        }
    }
}
