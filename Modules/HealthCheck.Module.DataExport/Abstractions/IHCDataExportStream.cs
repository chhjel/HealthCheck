using HealthCheck.Core.Models;
using HealthCheck.Core.Util;
using HealthCheck.Core.Util.Models;
using HealthCheck.Module.DataExport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Type of items returned.
        /// </summary>
        Type ItemType { get; }

        /// <summary>
        /// Type of custom parameters object if any.
        /// </summary>
        Type CustomParametersType { get; }

        /// <summary>
        /// Number of items to fetch per batch when exporting.
        /// </summary>
        int ExportBatchSize { get; }

        /// <summary>
        /// Max depth to search for members recursively.
        /// <para>Defaults to 4 if not specified.</para>
        /// </summary>
        int? MaxMemberDiscoveryDepth { get;  }

        /// <summary>
        /// If set to true, any property name can be entered and not only the ones from the model.
        /// </summary>
        bool AllowAnyPropertyName { get; }

        /// <summary>
        /// Optional filter what members to include.
        /// </summary>
        HCMemberFilterRecursive IncludedMemberFilter { get; }

        /// <summary>
        /// Defines what method to use for querying.
        /// </summary>
        QueryMethod Method { get; }

        /// <summary>
        /// True to show query input.
        /// <para>If true, the query input will be visible and value will be passed through the filter.</para>
        /// <para>If false, the query input will be hidden and null-values will be passed through the filter.</para>
        /// </summary>
        bool SupportsQuery();

        /// <summary>
        /// Formatters that can be selected per column.
        /// </summary>
        IEnumerable<IHCDataExportValueFormatter> ValueFormatters { get; }

        /// <summary>
        /// Get items to be filtered and exported. Invoked when <see cref="Method"/> is <see cref="QueryMethod.Queryable"/>.
        /// <para>For use when you have an IQueryable source.</para>
        /// </summary>
        Task<IQueryable> GetQueryableAsync();

        /// <summary>
        /// Get items to be filtered and exported. Invoked when <see cref="Method"/> is <see cref="QueryMethod.Enumerable"/>.
        /// </summary>
        /// <param name="filter">Input filter.</param>
        Task<EnumerableResult> GetEnumerableAsync(HCDataExportFilterData filter);

        /// <summary>
        /// Optionally format the value.
        /// </summary>
        /// <param name="propertyName">Dotted path to the member.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="value">Value of the property.</param>
        object DefaultFormatValue(string propertyName, Type propertyType, object value);

        /// <summary>
        /// Get the values of any addictional columns returned during query.
        /// </summary>
        Dictionary<string, object> GetAdditionalColumnValues(object item, List<string> includedProperties);

        /// <summary>
        /// Optional postprocessing of the data.
        /// </summary>
        List<HCBackendInputConfig> PostprocessCustomParameterDefinitions(List<HCBackendInputConfig> customParameterDefinitions);

        /// <summary>
        /// Result from <see cref="GetEnumerableAsync"/>
        /// </summary>
        public class EnumerableResult
        {
            /// <summary>
            /// Matching items for the given page.
            /// </summary>
            public System.Collections.IEnumerable PageItems { get; set; }

            /// <summary>
            /// Total match count.
            /// </summary>
            public int TotalCount { get; set; }

            /// <summary>
            /// Optional note.
            /// </summary>
            public string Note { get; set; }

            /// <summary>
            /// Optionally force result headers.
            /// </summary>
            public List<HCTypeNamePair> AdditionalColumns { get; set; }
        }

        /// <summary>
        /// What method to use for querying.
        /// </summary>
        public enum QueryMethod
        {
            /// <summary>
            /// Use the get queryable method.
            /// </summary>
            Queryable,

            /// <summary>
            /// Use the get enumerable method.
            /// </summary>
            Enumerable
        }
    }
}
