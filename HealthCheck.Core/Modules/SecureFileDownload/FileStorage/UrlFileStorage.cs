using HealthCheck.Core.Modules.SecureFileDownload.Abstractions;
using HealthCheck.Core.Modules.SecureFileDownload.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.SecureFileDownload.FileStorage
{
    /// <summary>
    /// Gets files from remote urls.
    /// </summary>
    public class UrlFileStorage : ISecureFileDownloadFileStorage
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
        public string FileIdInfo => $"Url to download file from.";

        /// <summary>
        /// Label to show in the management ui above file id field.
        /// </summary>
        public string FileIdLabel => "Absolute URL";

        /// <summary>
        /// Not supported for this implementation.
        /// </summary>
        public bool SupportsUpload => false;

        /// <summary>
        /// Gets files from remote urls.
        /// </summary>
        /// <param name="storageId">Id of this storage.</param>
        /// <param name="storageName">Name of this storage.</param>
        public UrlFileStorage(string storageId, string storageName)
        {
            StorageId = storageId;
            StorageName = storageName;
        }

        /// <summary>
        /// Get a stream to the given file, or null if found.
        /// </summary>
        /// <param name="fileId">Url to get file from.</param>
        public Stream GetFileStream(string fileId)
        {
            if (!HasFile(fileId))
            {
                return null;
            }

            var url = fileId;
            var webRequest = WebRequest.Create(url);
            var response = webRequest.GetResponse();
            var stream = response.GetResponseStream();
            return stream;
        }

        /// <summary>
        /// Check if the file is still available.
        /// </summary>
        /// <param name="fileId">Url to get file from.</param>
        public bool HasFile(string fileId)
        {
            try
            {
                var url = fileId;
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.Timeout = 3000;
                request.AllowAutoRedirect = false;
                request.Method = "HEAD";

                using (request.GetResponse())
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks that the id is a valid url.
        /// </summary>
        public virtual string ValidateFileIdBeforeSave(string fileId)
            => Uri.IsWellFormedUriString(fileId, UriKind.Absolute) ? null : $"'{fileId}' is did not validate as a url.";

        /// <summary>
        /// Returns null.
        /// </summary>
        public IEnumerable<HCSecureFileDownloadFileDetails> GetFileIdOptions() => Enumerable.Empty<HCSecureFileDownloadFileDetails>();

        /// <summary>
        /// Not supported for this implementation.
        /// </summary>
        public Task<HCSecureFileDownloadUploadResult> UploadFileAsync(Stream stream) => Task.FromResult(new HCSecureFileDownloadUploadResult
        {
            Success = false,
            FileId = null,
            ErrorMessage = "File upload not supported for this file storage."
        });
    }
}
