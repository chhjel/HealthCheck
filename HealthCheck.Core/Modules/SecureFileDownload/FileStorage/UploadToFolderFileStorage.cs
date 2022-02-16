using HealthCheck.Core.Modules.SecureFileDownload.Abstractions;
using HealthCheck.Core.Modules.SecureFileDownload.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.SecureFileDownload.FileStorage
{
    /// <summary>
    /// Upload files to and download from a folder.
    /// </summary>
    public class UploadToFolderFileStorage : ISecureFileDownloadFileStorage
    {
        /// <summary>
        /// Id of this storage.
        /// </summary>
        public string StorageId { get; set; }

        /// <summary>
        /// Name of this storage.
        /// </summary>
        public string StorageName { get; set; }

        /// <inheritdoc />
        public string FileIdInfo => "";

        /// <inheritdoc />
        public string FileIdLabel => "Select file";

        /// <summary>
        /// Root folder that files will be uploaded to. Should be a dedicated folder for file storage only.
        /// </summary>
        protected string Folder { get; private set; }

        /// <inheritdoc />
        public bool SupportsUpload => true;

        /// <summary>
        /// Gets files from a folder.
        /// </summary>
        /// <param name="storageId">Id of this storage.</param>
        /// <param name="storageName">Name of this storage.</param>
        /// <param name="folder">Root folder that files can be retrieved from. Should be a dedicated folder for file storage only.</param>
        public UploadToFolderFileStorage(string storageId, string storageName, string folder)
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
        public virtual IEnumerable<HCSecureFileDownloadFileDetails> GetFileIdOptions()
            => Directory.GetFiles(Folder, "*.*", SearchOption.AllDirectories)
                .Select(x => new HCSecureFileDownloadFileDetails
                {
                    Id = x.Substring(Folder.Length + 1),
                    Name = x.Substring(Folder.Length + 1)
                });

        /// <inheritdoc />
        public Task<HCSecureFileDownloadUploadResult> UploadFileAsync(Stream stream)
        {
            var filename = $"{Guid.NewGuid()}.HCUpload";
            var filepath = GetFilePath(filename);

            if (HasFile(filename))
            {
                return Task.FromResult(new HCSecureFileDownloadUploadResult { ErrorMessage = "A file with this name already exists." });
            }

            using (var fileStream = File.Create(filepath))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }

            return Task.FromResult(new HCSecureFileDownloadUploadResult
            {
                Success = true,
                FileId = filename
            });
        }

        private string GetFilePath(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return null;

            var path = Path.Combine(Folder, filename);
            path = Path.GetFullPath(path);
            if (!path.ToLower().Trim().StartsWith($"{Folder.ToLower()}\\"))
            {
                return null;
            }

            return path;
        }
    }
}
