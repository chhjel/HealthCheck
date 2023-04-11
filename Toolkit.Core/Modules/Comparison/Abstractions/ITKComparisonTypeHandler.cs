using QoDL.Toolkit.Core.Modules.Comparison.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.Comparison.Abstractions
{
    /// <summary>
    /// Handles instance selection and resolving.
    /// </summary>
    public interface ITKComparisonTypeHandler
    {
        /// <summary>
        /// The content type that will be returned from <see cref="GetInstanceWithIdAsync"/>.
        /// <para>Is passed to <see cref="ITKComparisonDiffer.CanHandle"/> to see if it can handle the type.</para>
        /// </summary>
        Type ContentType { get; }

        /// <summary>
        /// Name of the type to handle.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Optional description of the type to handle.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Optional placeholder in filter input in dialog where instances are selected.
        /// </summary>
        string FindInstanceSearchPlaceholder { get; }

        /// <summary>
        /// Optional description shown in dialog where instances are selected.
        /// </summary>
        string FindInstanceDescription { get; }

        /// <summary>
        /// Handle the input query and return a suitable amount of matching items.
        /// </summary>
        Task<List<TKComparisonInstanceSelection>> GetFilteredOptionsAsync(TKComparisonTypeFilter filter);

        /// <summary>
        /// Get an instance with the given id. The id is one that was returned from <see cref="GetFilteredOptionsAsync"/>.
        /// </summary>
        Task<object> GetInstanceWithIdAsync(string id);

        /// <summary>
        /// Get display name of the given instance that was returned from <see cref="GetInstanceWithIdAsync"/>.
        /// </summary>
        string GetInstanceDisplayName(object instance);
    }
}
