using QoDL.Toolkit.Core.Modules.SecureFileDownload.Abstractions;
using QoDL.Toolkit.Core.Modules.SecureFileDownload.Models;
using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.SecureFileDownload
{
    /// <summary>
    /// Options for <see cref="TKSecureFileDownloadModule"/>.
    /// </summary>
    public class TKSecureFileDownloadModuleOptions
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
        /// <para>Placeholders: [FILENAME]</para>
        /// </summary>
        public string DownloadPageTitle { get; set; } = "Download '[FILENAME]'";

        /// <summary>
        /// Duration of download tokens.
        /// <para>Defaults to 10 minutes.</para>
        /// </summary>
        public TimeSpan DownloadTokenLifetime { get; set; } = TimeSpan.FromMinutes(10);
    }
}
