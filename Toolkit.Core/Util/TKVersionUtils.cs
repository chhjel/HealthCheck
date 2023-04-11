using QoDL.Toolkit.Core.Config;
using System;
using System.IO;

namespace QoDL.Toolkit.Core.Util
{
    /// <summary>
    /// Things related to versioning.
    /// </summary>
    public static class TKVersionUtils
    {
        /// <summary>
        /// Checks if the version is different from the last time this method was called.
        /// <para>Should only return true the first time this or <see cref="ExecuteIfNewlyDeployedVersion"/> is executed with a new version.</para>
        /// <para>Any exception is ignored and will return false.</para>
        /// <para>Filepath used is %TEMP%\_tk_deployed_version_{key}.txt</para>
        /// </summary>
        /// <param name="key">Will be used as part of the filename. Should be unique per solution.</param>
        /// <param name="currentVersion">Current version to check against. Only used if <paramref name="method"/> is <see cref="NewlyDeployedVersionCheckMethod.FileContent"/></param>
        /// <param name="method">Logic to use to check for new version.</param>
        public static bool IsNewlyDeployedVersion(string key, string currentVersion, NewlyDeployedVersionCheckMethod method = NewlyDeployedVersionCheckMethod.FileContent)
        {
            try
            {
                var filePath = Path.Combine(Path.GetTempPath(), TKIOUtils.SanitizeFilename($"_tk_deployed_version_{key}") + ".txt");
                if (method == NewlyDeployedVersionCheckMethod.FileExistence)
                {
                    var result = !File.Exists(filePath);
                    File.WriteAllText(filePath, currentVersion);
                    return result;
                }
                else if (method == NewlyDeployedVersionCheckMethod.FileContent)
                {
                    if (!File.Exists(filePath) || File.ReadAllText(filePath).Trim() != currentVersion.Trim())
                    {
                        File.WriteAllText(filePath, currentVersion);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKVersionUtils), nameof(IsNewlyDeployedVersion), ex);
                return false;
            }
        }

        /// <summary>
        /// Executes the given action the first time this method or <see cref="IsNewlyDeployedVersion"/> is invoked if the version is different from the last time this method was called.
        /// <para>Any exception is ignored and won't execute the action.</para>
        /// <para>Filepath used is %TEMP%\_tk_deployed_version_{key}.txt</para>
        /// </summary>
        /// <param name="key">Will be used as part of the filename. Should be unique per solution.</param>
        /// <param name="currentVersion">Current version to check against. Only used if <paramref name="method"/> is <see cref="NewlyDeployedVersionCheckMethod.FileContent"/></param>
        /// <param name="actionOnNewVersion">Action to execute if a new version was found. Parameter is the value of <paramref name="currentVersion"/>.</param>
        /// <param name="method">Logic to use to check for new version.</param>
        public static void ExecuteIfNewlyDeployedVersion(string key, string currentVersion, Action<string> actionOnNewVersion, NewlyDeployedVersionCheckMethod method = NewlyDeployedVersionCheckMethod.FileContent)
        {
            try
            {
                if (IsNewlyDeployedVersion(key, currentVersion, method))
                {
                    actionOnNewVersion.Invoke(currentVersion);
                }
            }
            catch(Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKVersionUtils), nameof(ExecuteIfNewlyDeployedVersion), ex);
            }
        }

        /// <summary>
        /// How to check for newly deployed version.
        /// </summary>
        public enum NewlyDeployedVersionCheckMethod
        {
            /// <summary>
            /// New version will be detected when a temp file either is missing or does not contain the given version.
            /// <para>Writes a temp file with the given version if missing or contains a different version.</para>
            /// </summary>
            FileContent,

            /// <summary>
            /// New version will be detected when a temp file is missing.
            /// <para>Writes a temp file with the given version if missing.</para>
            /// </summary>
            FileExistence
        }
    }
}
