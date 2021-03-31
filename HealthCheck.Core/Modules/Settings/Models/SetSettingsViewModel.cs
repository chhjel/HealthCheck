﻿using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Settings.Models
{
    /// <summary>
    /// Model used when updating settings.
    /// </summary>
    public class SetSettingsViewModel
    {
        /// <summary>
        /// Settings that will be set.
        /// </summary>
        public Dictionary<string, string> Values { get; set; }
    }
}
