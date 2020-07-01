using HealthCheck.Core.Modules.SecureFileDownload.Abstractions;
using System.IO;

namespace HealthCheck.Core.Modules.SecureFileDownload.FileStorage
{
    /// <summary>
    /// Gets files from a folder.
    /// </summary>
    public class FolderFileStorage : ISecureFileDownloadFileStorage
    {
        /// <summary>
        /// Id of this storage.
        /// </summary>
        public string StorageId { get; set; }

        /// <summary>
        /// Root folder that files can be retrieved from.
        /// </summary>
        protected string Folder { get; private set; }

        /// <summary>
        /// Gets files from a folder.
        /// </summary>
        /// <param name="storageId">Id of this storage.</param>
        /// <param name="folder">Root folder that files can be retrieved from.</param>
        public FolderFileStorage(string storageId, string folder)
        {
            StorageId = storageId;
            Folder = Path.GetFullPath(folder).TrimEnd('/').Trim();
        }

        /// <summary>
        /// Get a stream to the given file, or null if found.
        /// </summary>
        /// <param name="fileId">Relative path below <see cref="Folder"/>.</param>
        public Stream GetFileStream(string fileId)
        {
            if (!HasFile(fileId))
            {
                return null;
            }

            var path = GetFilePath(fileId);
            return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        /// <summary>
        /// Check if the file is still available.
        /// </summary>
        /// <param name="fileId">Relative path below <see cref="Folder"/>.</param>
        public bool HasFile(string fileId)
        {
            var path = GetFilePath(fileId);
            if (path == null)
            {
                return false;
            }
            else
            {
                return File.Exists(path);
            }
        }

        private string GetFilePath(string fileId)
        {
            var path = Path.Combine(Folder, fileId);
            path = Path.GetFullPath(path);
            if (!path.ToLower().Trim().StartsWith(Folder.ToLower()))
            {
                return null;
            }

            return path;
        }
    }
}
