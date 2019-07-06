﻿namespace HealthCheck.Core.Entities
{
    /// <summary>
    /// Options for a test set group.
    /// </summary>
    public class TestSetGroupOptions
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
