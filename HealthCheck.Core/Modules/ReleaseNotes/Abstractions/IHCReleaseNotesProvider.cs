using HealthCheck.Core.Modules.ReleaseNotes.Models;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.ReleaseNotes.Abstractions
{
    /// <summary>
    /// Provides data to display in the release notes module.
    /// </summary>
    public interface IHCReleaseNotesProvider
    {
        /// <summary>
        /// Retrieve release notes to display.
        /// </summary>
        Task<HCReleaseNotesViewModels> GetViewModelAsync();
    }
}
