using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

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
            var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using ReverseStreamReader streamReader = new ReverseStreamReader(fileStream);
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                yield return line;
            }
        }

        /// <summary>
        /// Read file contents. Allows reading when file is in use.
        /// </summary>
        public static string ReadFile(string filePath)
        {
            var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using StreamReader reader = new StreamReader(fileStream);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Strip illegal chars and reserved words from a given filename (not full path, only filename).
        /// </summary>
        public static string SanitizeFilename(string filename)
        {
            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidReStr = $@"[{invalidChars}]+";

            var sanitisedNamePart = Regex.Replace(filename, invalidReStr, "_");
            foreach (var reservedWord in _reservedFilenames)
            {
                var reservedWordPattern = $"^{reservedWord}\\.";
                sanitisedNamePart = Regex.Replace(sanitisedNamePart, reservedWordPattern, "_reservedWord_.", RegexOptions.IgnoreCase);
            }

            return sanitisedNamePart;
        }

        private static readonly string[] _reservedFilenames = new[]
        {
            "CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4",
            "COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4",
            "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };
}
}
