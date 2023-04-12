namespace QoDL.Toolkit.Core.Modules.SecureFileDownload.Models;

/// <summary>
/// Result model from <see cref="TKSecureFileDownloadModule.SaveDefinition"/>.
/// </summary>
public class SecureFileDownloadSaveViewModel
{
    /// <summary>
    /// True if saved succesfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error if any.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// The saved definition.
    /// </summary>
    public SecureFileDownloadDefinition Definition { get; set; }
}
