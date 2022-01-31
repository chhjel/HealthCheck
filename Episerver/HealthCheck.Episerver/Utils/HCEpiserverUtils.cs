using EPiServer.Data.Dynamic;
using HealthCheck.Core.Config;
using HealthCheck.Core.Util;
using HealthCheck.Episerver.Models;
using HealthCheck.Episerver.Storage;
using System;
using System.Linq;

namespace HealthCheck.Episerver.Utils
{
    /// <summary>
    /// Various episerver utilities.
    /// </summary>
    public static class HCEpiserverUtils
    {
        /// <summary>
        /// Checks if the given version is different from one stored in epi DDS.
        /// <para>If the version differs the new version will be written to DDS and the given action invoked.</para>
        /// <para>Use to execute things once per deployed version.</para>
        /// <para>DDS model type: <see cref="HCLastCheckedVersionData"/></para>
        /// </summary>
        /// <param name="currentVersion">Current version to check against.</param>
        /// <param name="actionOnNewVersion">Action to execute if a new version was found. Parameter is the value of <paramref name="currentVersion"/>.</param>
        public static void ExecuteIfNewlyDeployedVersion(string currentVersion, Action<string> actionOnNewVersion)
        {
            try
            {
                var store = typeof(HCLastCheckedVersionData).GetOrCreateStore();
                var versionData = store.Items<HCLastCheckedVersionData>().FirstOrDefault();

                if (versionData == null)
                {
                    var data = new HCLastCheckedVersionData { LastCheckedVersion = currentVersion };
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
                HCGlobalConfig.OnExceptionEvent?.Invoke(typeof(HCEpiserverUtils), nameof(ExecuteIfNewlyDeployedVersion), ex);
            }
        }

        /// <summary>
        /// Shortcut to <see cref="HCSelfUptimeChecker.EnsureIntervalCheckStarted"/> using a storage implementation using epi DDS.
        /// <para>Storage implementation used: <see cref="HCEpiserverDDSSelfUptimeCheckerStorage"/>.</para>
        /// </summary>
		/// <param name="checkInterval">How often to store a timestamp into storage. E.g. 2 minutes.</param>
		/// <param name="failIfNoCheckForDuration">If the duration between the last stored and the current time is greater than this, <paramref name="onDowntimeDetected"/> will be invoked. E.g. 5 minutes.</param>
		/// <param name="onDowntimeDetected">Callback for when downtime was detected. Will be invoked after the site is back up.</param>
        public static void StartSelfUptimeChecker(TimeSpan checkInterval, TimeSpan failIfNoCheckForDuration, HCSelfUptimeChecker.OnBackUpAfterDowntime onDowntimeDetected)
        {
            try
            {
                var storage = new HCEpiserverDDSSelfUptimeCheckerStorage();
                HCSelfUptimeChecker.EnsureIntervalCheckStarted(storage, checkInterval, failIfNoCheckForDuration, onDowntimeDetected);
            }
            catch (Exception ex)
            {
                HCGlobalConfig.OnExceptionEvent?.Invoke(typeof(HCEpiserverUtils), nameof(StartSelfUptimeChecker), ex);
            }
        }
    }
}
