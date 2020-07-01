using System.IO;

namespace HealthCheck.Core.Modules.SecureFileDownload.Abstractions
{
    /// <summary>
    /// Provides file streams for download.
    /// </summary>
    public interface ISecureFileDownloadFileStorage
    {
        /// <summary>
        /// Unique id.
        /// </summary>
        string StorageId { get; }

        /// <summary>
        /// Returns true if the file is present and can be retrieved through <see cref="GetFileStream(string)"/>.
        /// </summary>
        bool HasFile(string fileId);

        /// <summary>
        /// Get the stream to a file by its id, or null if it does not exist.
        /// </summary>
        Stream GetFileStream(string fileId);
    }
}
