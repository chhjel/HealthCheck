using System.Collections.Generic;

namespace HealthCheck.Core.Modules.SecureFileDownload.Models
{
    /// <summary>
    /// Result model from <see cref="HCSecureFileDownloadModule.GetDownloads"/>.
    /// </summary>
    public class SecureFileDownloadsViewModel
    {
        /// <summary>
        /// All stored download definitions.
        /// </summary>
        public IEnumerable<SecureFileDownloadDefinition> Definitions { get; set; }
    }
}
