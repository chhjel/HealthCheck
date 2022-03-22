﻿using HealthCheck.Core.Modules.Dataflow.Abstractions;
using HealthCheck.Core.Util;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Dataflow.Models
{
    /// <summary>
    /// Metadata describing an <see cref="IHCDataflowUnifiedSearch{TAccessRole}"/>.
    /// </summary>
    public class DataflowUnifiedSearchMetadata<TAccessRole>
    {
        /// <summary>
        /// Unique id of the stream.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the stream to show in the UI.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the stream to show in the UI.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Placeholder for the search input field.
        /// </summary>
        public string QueryPlaceholder { get; set; }

        /// <summary>
        /// Optionally group the stream within the given group name.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Optionally set roles that have access to this stream.
        /// </summary>
        public Maybe<TAccessRole> RolesWithAccess { get; set; }

        /// <summary></summary>
        public Dictionary<string, string> GroupByStreamNamesOverrides { get; set; }

        /// <summary></summary>
        public Dictionary<string, string> StreamNamesOverrides { get; set; }

        /// <summary>
        /// Optionally label each grouped item.
        /// <para>[KEY] can be used as a placeholder for the grouped key value.</para>
        /// </summary>
        public string GroupByLabel { get; set; }
    }
}