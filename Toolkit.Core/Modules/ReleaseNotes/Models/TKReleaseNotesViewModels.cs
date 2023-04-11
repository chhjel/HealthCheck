namespace QoDL.Toolkit.Core.Modules.ReleaseNotes.Models
{
    /// <summary>
    /// Contains data to display in the release notes module.
    /// </summary>
    public class TKReleaseNotesViewModels
    {
        /// <summary>
        /// Version without dev details.
        /// </summary>
        public TKReleaseNotesViewModel WithoutDevDetails { get; set; }

        /// <summary>
        /// Version with dev details.
        /// </summary>
        public TKReleaseNotesViewModel WithDevDetails { get; set; }

        /// <summary>
        /// Creates both models, one with dev details included.
        /// </summary>
        public static TKReleaseNotesViewModels CreateError(string message, string devDetails = null)
        {
            return new TKReleaseNotesViewModels
            {
                WithoutDevDetails = new TKReleaseNotesViewModel { ErrorMessage = message },
                WithDevDetails = new TKReleaseNotesViewModel { ErrorMessage = message + " " + devDetails }
            };
        }
    }
}
