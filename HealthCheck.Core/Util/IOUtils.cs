using System.Collections.Generic;
using System.IO;

namespace HealthCheck.Core.Util
{
    /// <summary>
    /// IO related utils.
    /// </summary>
    public static class IOUtils
    {
        /// <summary>
        /// Read file one line at a time. Allows reading when file is in use.
        /// </summary>
        public static IEnumerable<string> ReadLines(string filePath)
        {
            using (FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (ReverseStreamReader streamReader = new ReverseStreamReader(fileStream))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        /// <summary>
        /// Read file contents. Allows reading when file is in use.
        /// </summary>
        public static string ReadFile(string filePath)
        {
            using (FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(fileStream))
            {
                return reader.ReadToEnd();
            };
        }
    }
}
