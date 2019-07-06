namespace HealthCheck.WebUI.ViewModels
{
    /// <summary>
    /// Options for test set groups.
    /// </summary>
    public class GroupOptionsViewModel
    {
        /// <summary>
        /// Name of group to set options for.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Icon override.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Order in the list.
        /// </summary>
        public int UIOrder { get; set; }
    }
}
