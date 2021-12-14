namespace HealthCheck.Core.Modules.SecureFileDownload.Models
{
    /// <summary>
    /// Response from file upload.
    /// </summary>
    public class HCSecureFileDownloadUploadResult
    {
        /// <summary></summary>
        public bool Success { get; set; }

        /// <summary></summary>
        public string ErrorMessage { get; set; }

        /// <summary></summary>
        public string FileId { get; set; }
    }
}
