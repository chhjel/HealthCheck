namespace HealthCheck.Core.Modules.ReleaseNotes.Models
{
    /// <summary>
    /// Contains data to display in the release notes module.
    /// </summary>
    public class HCReleaseNotesViewModels
    {
        /// <summary>
        /// Version without dev details.
        /// </summary>
        public HCReleaseNotesViewModel WithoutDevDetails { get; set; }

        /// <summary>
        /// Version with dev details.
        /// </summary>
        public HCReleaseNotesViewModel WithDevDetails { get; set; }

        /// <summary>
        /// Creates both models, one with dev details included.
        /// </summary>
        public static HCReleaseNotesViewModels CreateError(string message, string devDetails = null)
        {
            return new HCReleaseNotesViewModels
            {
                WithoutDevDetails = new HCReleaseNotesViewModel { ErrorMessage = message },
                WithDevDetails = new HCReleaseNotesViewModel { ErrorMessage = message + " " + devDetails }
            };
        }
    }
}
