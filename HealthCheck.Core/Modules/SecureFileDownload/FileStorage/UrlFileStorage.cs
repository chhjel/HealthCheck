using HealthCheck.Core.Modules.SecureFileDownload.Abstractions;
using System.IO;
using System.Net;

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
        /// Gets files from remote urls.
        /// </summary>
        /// <param name="storageId">Id of this storage.</param>
        public UrlFileStorage(string storageId)
        {
            StorageId = storageId;
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
    }
}
