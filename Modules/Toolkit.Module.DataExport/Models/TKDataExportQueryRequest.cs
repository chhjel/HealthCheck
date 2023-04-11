using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Module.DataExport.Abstractions;
using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Module.DataExport.Models
{
    /// <summary>
    /// Model sent to <see cref="ITKDataExportService.QueryAsync"/>
    /// </summary>
    public class TKDataExportQueryRequest
    {
        /// <summary>
        /// Type of the stream.
        /// </summary>
        public string StreamId { get; set; }

        /// <summary>
        /// Page index to start at.
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Page size to return.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Linq query.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Optional preset id, overrides <see cref="Query"/>, <see cref="IncludedProperties"/> and <see cref="HeaderNameOverrides"/> if given.
        /// </summary>
        [TKRtProperty(ForcedNullable = true)]
        public Guid? PresetId { get; set; }

        /// <summary>
        /// What properties to include.
        /// </summary>
        public List<string> IncludedProperties { get; set; }

        /// <summary>
        /// Any header renames.
        /// <para>Only used for export.</para>
        /// </summary>
        public Dictionary<string, string> HeaderNameOverrides { get; set; }

        /// <summary>
        /// Id of exporter to use.
        /// </summary>
        public string ExporterId { get; set; }

        /// <summary>
        /// Any custom parameters.
        /// </summary>
        public Dictionary<string, string> CustomParameters { get; set; }

        /// <summary>
        /// Config for any selected formatters.
        /// </summary>
        public Dictionary<string, TKDataExportValueFormatterConfig> ValueFormatterConfigs { get; set; }

        /// <summary>
        /// Any custom columns.
        /// </summary>
        public Dictionary<string, string> CustomColumns { get; set; }
    }
}
