namespace QoDL.Toolkit.Core.Modules.LogViewer.Models
{
    internal class LogFolderSource
    {
        public string Directory { get; set; }
        public string FileFilter { get; set; }
        public bool Recursive { get; set; }

        public LogFolderSource(string directory, string fileFilter = "*", bool recursive = true)
        {
            Directory = directory;
            FileFilter = fileFilter;
            Recursive = recursive;
        }
    }
}