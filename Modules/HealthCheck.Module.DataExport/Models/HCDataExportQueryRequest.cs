using HealthCheck.Core.Attributes;
using HealthCheck.Module.DataExport.Abstractions;
using System;
using System.Collections.Generic;

namespace HealthCheck.Module.DataExport.Models
{
    /// <summary>
    /// Model sent to <see cref="IHCDataExportService.QueryAsync"/>
    /// </summary>
    public class HCDataExportQueryRequest
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
        [HCRtProperty(ForcedNullable = true)]
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
    }
}
