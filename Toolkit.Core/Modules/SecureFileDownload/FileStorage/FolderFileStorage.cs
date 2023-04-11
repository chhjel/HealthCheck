using QoDL.Toolkit.Core.Modules.SecureFileDownload.Abstractions;
using QoDL.Toolkit.Core.Modules.SecureFileDownload.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.SecureFileDownload.FileStorage
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
        /// Can be set to true to pick files from the selected folder.
        /// </summary>
        public bool SupportsSelectingFile { get; set; } = false;

        /// <summary>
        /// Can be set to false to disable upload.
        /// </summary>
        public bool SupportsUpload { get; set; } = true;

        /// <summary>
        /// Gets files from a folder.
        /// </summary>
        /// <param name="storageId">Id of this storage.</param>
        /// <param name="storageName">Name of this storage.</param>
        /// <param name="folder">Root folder that files can be retrieved from. Should be a dedicated folder for file storage only.</param>
        /// <param name="allowUpload">Allows uploading file to the given folder as well.</param>
        public FolderFileStorage(string storageId, string storageName, string folder, bool allowUpload = true)
        {
            StorageId = storageId;
            StorageName = storageName;
            Folder = Path.GetFullPath(folder).TrimEnd('/', '\\').Trim();
            SupportsUpload = allowUpload;
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
            => (SupportsUpload || HasFile(fileId)) ? null : $"File was not found at '{GetFilePath(fileId)}.'";

        /// <summary>
        /// Returns a list of all the files below the folder.
        /// </summary>
        public virtual IEnumerable<TKSecureFileDownloadFileDetails> GetFileIdOptions()
            => Directory.GetFiles(Folder, "*.*", SearchOption.AllDirectories)
                .Where(x => !x.EndsWith(".TKUpload"))
                .Select(x => new TKSecureFileDownloadFileDetails
                {
                    Id = x.Substring(Folder.Length + 1),
                    Name = x.Substring(Folder.Length + 1)
                });

        /// <inheritdoc />
        public async Task<TKSecureFileDownloadUploadResult> UploadFileAsync(Stream stream)
        {
            var filename = $"{Guid.NewGuid()}.TKUpload";
            var filepath = GetFilePath(filename);

            if (HasFile(filename))
            {
                return new TKSecureFileDownloadUploadResult { ErrorMessage = "A file with this name already exists." };
            }

            using (var fileStream = File.Create(filepath))
            {
                stream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(fileStream).ConfigureAwait(false);
            }

            return new TKSecureFileDownloadUploadResult
            {
                Success = true,
                FileId = filename
            };
        }

        /// <inheritdoc />
        public Task<bool> DeleteUploadedFileAsync(string fileId)
        {
            if (!HasFile(fileId)) return Task.FromResult(true);

            try
            {
                var path = GetFilePath(fileId);
                File.Delete(path);
            }
            catch (Exception)
            {
                Task.FromResult(false);
            }
            return Task.FromResult(true);
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
