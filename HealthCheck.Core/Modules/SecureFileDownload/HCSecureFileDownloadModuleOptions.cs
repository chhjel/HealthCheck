using HealthCheck.Core.Modules.SecureFileDownload.Abstractions;
using HealthCheck.Core.Modules.SecureFileDownload.Models;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.SecureFileDownload
{
    /// <summary>
    /// Options for <see cref="HCSecureFileDownloadModule"/>.
    /// </summary>
    public class HCSecureFileDownloadModuleOptions
    {
        /// <summary>
        /// Provides <see cref="SecureFileDownloadDefinition"/>s.
        /// </summary>
        public ISecureFileDownloadDefinitionStorage DefinitionStorage { get; set; }

        /// <summary>
        /// Locations to download files from.
        /// </summary>
        public IEnumerable<ISecureFileDownloadFileStorage> FileStorages { get; set; }

        /// <summary>
        /// Title of the download page.
        /// </summary>
        public string DownloadPageTitle { get; set; } = "Downloads";
    }
}
