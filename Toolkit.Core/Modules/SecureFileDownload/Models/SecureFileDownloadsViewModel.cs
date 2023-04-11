using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.SecureFileDownload.Models
{
    /// <summary>
    /// Result model from <see cref="TKSecureFileDownloadModule.GetDownloads"/>.
    /// </summary>
    public class SecureFileDownloadsViewModel
    {
        /// <summary>
        /// All stored download definitions.
        /// </summary>
        public IEnumerable<SecureFileDownloadDefinition> Definitions { get; set; }
    }
}
