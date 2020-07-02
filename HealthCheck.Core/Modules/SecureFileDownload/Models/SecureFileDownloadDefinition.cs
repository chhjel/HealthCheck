using HealthCheck.Core.Modules.SecureFileDownload.Abstractions;
using System;

namespace HealthCheck.Core.Modules.SecureFileDownload.Models
{
    /// <summary>
    /// Represents a possible download link.
    /// </summary>
    public class SecureFileDownloadDefinition
    {
        /// <summary>
        /// Definition id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Time when this definition was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Name of the user that created this definition.
        /// </summary>
        public string CreatedByUsername { get; set; }

        /// <summary>
        /// Id of the user that created this definition.
        /// </summary>
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// Time when this definition was last modified.
        /// </summary>
        public DateTimeOffset LastModifiedAt { get; set; }

        /// <summary>
        /// Name of the user that last modified this definition.
        /// </summary>
        public string LastModifiedByUsername { get; set; }

        /// <summary>
        /// Id of the user that last modified this definition.
        /// </summary>
        public string LastModifiedByUserId { get; set; }

        /// <summary>
        /// Name of the file to download.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Id of the <see cref="ISecureFileDownloadFileStorage"/> implementation that is used to retrieve the file.
        /// </summary>
        public string StorageId { get; set; }

        /// <summary>
        /// Id of the file to download.
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        /// Text in the url for this download.
        /// </summary>
        public string UrlSegmentText { get; set; }

        /// <summary>
        /// Optional password required to download the file.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Number of times file was downloaded.
        /// </summary>
        public int DownloadCount { get; set; }

        /// <summary>
        /// Latest time the file was downloaded.
        /// </summary>
        public DateTimeOffset? LastDownloadedAt { get; set; }

        /// <summary>
        /// Max number of times the file can be downloaded.
        /// </summary>
        public int? DownloadCountLimit { get; set; }

        /// <summary>
        /// Time when the file will no longer be available for download.
        /// </summary>
        public DateTimeOffset? ExpiresAt { get; set; }

        /// <summary>
        /// A note that is displayed on the download page.
        /// </summary>
        public string Note { get; set; }
    }
}
