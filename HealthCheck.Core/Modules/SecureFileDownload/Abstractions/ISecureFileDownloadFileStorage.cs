using HealthCheck.Core.Modules.SecureFileDownload.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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
        /// Name to show in the management ui for this instance.
        /// </summary>
        string StorageName { get; }

        /// <summary>
        /// Label to show in the management ui above file id/upload field.
        /// </summary>
        string FileIdLabel { get; }

        /// <summary>
        /// Description to show in the management ui that explains what to enter as file id.
        /// </summary>
        string FileIdInfo { get; }

        /// <summary>
        /// If true, the upload function will be available and will invoke <see cref="UploadFileAsync"/>.
        /// </summary>
        bool SupportsUpload { get; }

        /// <summary>
        /// Returns true if the file is present and can be retrieved through <see cref="GetFileStream(string)"/>.
        /// </summary>
        bool HasFile(string fileId);

        /// <summary>
        /// Called when the user uploads a file if supported.
        /// </summary>
        Task<HCSecureFileDownloadUploadResult> UploadFileAsync(Stream stream);

        /// <summary>
        /// Get the stream to a file by its id, or null if it does not exist.
        /// </summary>
        Stream GetFileStream(string fileId);

        /// <summary>
        /// Validate file id from the UI and return an error message if it's not valid.
        /// <para>Returns null if allowed.</para>
        /// </summary>
        string ValidateFileIdBeforeSave(string fileId);

        /// <summary>
        /// Optionally return a list of possible choices.
        /// <para>If not null or empty then only the returned values can be selected in the UI.</para>
        /// </summary>
        IEnumerable<HCSecureFileDownloadFileDetails> GetFileIdOptions();
    }
}
