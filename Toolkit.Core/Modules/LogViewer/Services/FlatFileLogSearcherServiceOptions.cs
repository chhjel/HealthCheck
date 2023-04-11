using QoDL.Toolkit.Core.Modules.LogViewer.Models;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.LogViewer.Services
{
    /// <summary>
    /// Options for <see cref="FlatFileLogSearcherService"/>.
    /// </summary>
    public class FlatFileLogSearcherServiceOptions
    {
        internal List<LogFolderSource> LogFolders { get; set; } = new List<LogFolderSource>();
        internal List<string> LogFilepaths { get; set; } = new List<string>();

        /// <summary>
        /// Include the given filepaths in log searches.
        /// </summary>
        public FlatFileLogSearcherServiceOptions IncludeLogFiles(params string[] filepaths)
        {
            LogFilepaths.AddRange(filepaths);
            return this;
        }

        /// <summary>
        /// Include logs in the given folder in log searches.
        /// </summary>
        public FlatFileLogSearcherServiceOptions IncludeLogFilesInDirectory(string directory, string filter = "*", bool recursive = true)
        {
            LogFolders.Add(new LogFolderSource(directory, filter, recursive));
            return this;
        }

        /// <summary>
        /// Include logs in the given folders in log searches.
        /// </summary>
        public FlatFileLogSearcherServiceOptions IncludeLogFilesInDirectories(IEnumerable<string> directories, string filter = "*", bool recursive = true)
        {
            foreach (var directory in directories)
            {
                LogFolders.Add(new LogFolderSource(directory, filter, recursive));
            }
            return this;
        }
    }
}
