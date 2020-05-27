using System;

namespace HealthCheck.Core.Modules.Settings.Attributes
{
    /// <summary>
    /// Apply to a property to customize how it will look in the healthcheck settings page.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class HealthCheckSettingAttribute : Attribute
    {
        /// <summary>
        /// Defaults to property name prettified.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Information about this setting.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Settings will be grouped by this value.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Apply to a property to customize how it will look in the healthcheck settings page.
        /// </summary>
        /// <param name="displayName">Defaults to property name prettified.</param>
        /// <param name="description">Information about this setting.</param>
        /// <param name="groupName">Settings will be grouped by this value.</param>
        public HealthCheckSettingAttribute(string displayName = null, string description = null, string groupName = null)
        {
            DisplayName = displayName;
            Description = description;
            GroupName = groupName;
        }
    }
}
