﻿using System;
using System.Collections.Generic;

namespace HealthCheck.Module.DataExport.Models
{
    /// <summary></summary>
    public class HCDataExportStreamQueryPreset
    {
        /// <summary></summary>
        public Guid Id { get; set; }

        /// <summary></summary>
        public string StreamId { get; set; }

        /// <summary>
        /// Name of the preset.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Optional description of the preset.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Query to apply.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Properties to include in configured order.
        /// </summary>
        public List<string> IncludedProperties { get; set; }

        /// <summary>
        /// Any header renames.
        /// </summary>
        public Dictionary<string, string> HeaderNameOverrides { get; set; }

        /// <summary>
        /// Any custom parameters.
        /// </summary>
        public Dictionary<string, string> CustomParameters { get; set; }

        /// <summary>
        /// Config for any selected formatters.
        /// </summary>
        public Dictionary<string, HCDataExportValueFormatterConfig> ValueFormatterConfigs { get; set; }

        /// <summary>
        /// Any custom columns.
        /// </summary>
        public Dictionary<string, string> CustomColumns { get; set; }
    }
}
