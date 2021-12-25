using EPiServer.Data.Dynamic;
using HealthCheck.Core.Config;
using HealthCheck.Episerver.Models;
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
    }
}
