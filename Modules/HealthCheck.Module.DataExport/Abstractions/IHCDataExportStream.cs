using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Module.DataExport.Abstractions
{
    /// <summary>
    /// Represents a stream of items that can be filtered and exported upon.
    /// </summary>
    public interface IHCDataExportStream
    {
        /// <summary>
        /// Name of the stream.
        /// </summary>
        string StreamDisplayName { get; }

        /// <summary>
        /// Optional description of the stream. Supports HTML.
        /// </summary>
        string StreamDescription { get; }

        /// <summary>
        /// Name of the stream group if any.
        /// </summary>
        string StreamGroupName { get; }

        /// <summary>
        /// Optional access roles that can access this stream.
        /// <para>Must be a flags enum of the same type as the one used on the healthcheck controller.</para>
        /// </summary>
        object AllowedAccessRoles { get; }

        /// <summary>
        /// Optional categories this stream belongs to.
        /// <para>Can be used for more granular access configuration.</para>
        /// </summary>
        List<string> Categories { get; }

        /// <summary>
        /// Type of items returned from <see cref="GetQueryableAsync"/>
        /// </summary>
        Type ItemType { get; }

        /// <summary>
        /// Get items to be filtered and exported.
        /// <para>Must return an IQueryable{T} </para>
        /// </summary>
        Task<object> GetQueryableAsync();
    }
}
