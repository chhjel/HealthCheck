namespace HealthCheck.Core.Modules.Settings.Models
{
    /// <summary>
    /// A setting to be displayed in the settings page in the healthcheck ui.
    /// </summary>
    public class HealthCheckSetting
    {
        /// <summary>
        /// Unique id of the setting.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the setting in the ui.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Optional description of the setting.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Typename of the setting. 'String', 'Int32' or 'Boolean'.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Value of the setting.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Group the setting belongs to.
        /// </summary>
        public string GroupName { get; set; }
    }
}
