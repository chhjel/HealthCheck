using QoDL.Toolkit.Core.Modules.ReleaseNotes.Models;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.ReleaseNotes.Abstractions
{
    /// <summary>
    /// Provides data to display in the release notes module.
    /// </summary>
    public interface ITKReleaseNotesProvider
    {
        /// <summary>
        /// Retrieve release notes to display.
        /// </summary>
        Task<TKReleaseNotesViewModels> GetViewModelAsync();
    }
}
