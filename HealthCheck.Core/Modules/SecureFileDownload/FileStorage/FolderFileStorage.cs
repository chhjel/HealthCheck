using HealthCheck.Core.Modules.SecureFileDownload.Abstractions;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        /// Name of this storage.
        /// </summary>
        public string StorageName { get; set; }

        /// <summary>
        /// Description to show in the management ui that explains what to enter as file id.
        /// </summary>
        public string FileIdInfo => $"Relative file path below the folder '{Folder}'.";

        /// <summary>
        /// Label to show in the management ui above file id field.
        /// </summary>
        public string FileIdLabel => "Relative file path";

        /// <summary>
        /// Root folder that files can be retrieved from. Should be a dedicated folder for file storage only.
        /// </summary>
        protected string Folder { get; private set; }

        /// <summary>
        /// Gets files from a folder.
        /// </summary>
        /// <param name="storageId">Id of this storage.</param>
        /// <param name="storageName">Name of this storage.</param>
        /// <param name="folder">Root folder that files can be retrieved from. Should be a dedicated folder for file storage only.</param>
        public FolderFileStorage(string storageId, string storageName, string folder)
        {
            StorageId = storageId;
            StorageName = storageName;
            Folder = Path.GetFullPath(folder).TrimEnd('/', '\\').Trim();
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

        /// <summary>
        /// Validate file id from the UI and return an error message if it's not valid.
        /// <para>Checks that the file exists on disk.</para>
        /// <para>Returns null if allowed.</para>
        /// </summary>
        public virtual string ValidateFileIdBeforeSave(string fileId)
            => HasFile(fileId) ? null : $"File was not found at '{GetFilePath(fileId)}.'";

        /// <summary>
        /// Returns a list of all the files below the folder.
        /// </summary>
        public virtual IEnumerable<string> GetFileIdOptions()
            => Directory.GetFiles(Folder, "*.*", SearchOption.AllDirectories)
                .Select(x => x.Substring(Folder.Length + 1));

        private string GetFilePath(string fileId)
        {
            var path = Path.Combine(Folder, fileId);
            path = Path.GetFullPath(path);
            if (!path.ToLower().Trim().StartsWith($"{Folder.ToLower()}\\"))
            {
                return null;
            }

            return path;
        }
    }
}
