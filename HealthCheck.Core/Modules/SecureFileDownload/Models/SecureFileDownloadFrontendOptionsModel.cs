using System.Collections.Generic;

namespace HealthCheck.Core.Modules.SecureFileDownload.Models
{
    /// <summary>
    /// Front-end options object for this module.
    /// </summary>
    public class SecureFileDownloadFrontendOptionsModel
    {
        /// <summary>
        /// Describes storage implementations.
        /// </summary>
        public List<SecureFileDownloadStorageInfo> StorageInfos { get; set; } = new List<SecureFileDownloadStorageInfo>();
    }

    /// <summary>
    /// Describes a storage implementation.
    /// </summary>
    public class SecureFileDownloadStorageInfo
    {
        /// <summary>
        /// Id of the storage implementation.
        /// </summary>
        public string StorageId { get; set; }

        /// <summary>
        /// Name of the storage implementation.
        /// </summary>
        public string StorageName { get; set; }

        /// <summary>
        /// Information about how the file id is used for this storage.
        /// </summary>
        public string FileIdInfo { get; set; }

        /// <summary>
        /// Label to show in the management ui above file id field.
        /// </summary>
        public string FileIdLabel { get; set; }

        /// <summary>
        /// If upload of new files is supported or not.
        /// </summary>
        public bool SupportsUpload { get; set; }
    }
}
