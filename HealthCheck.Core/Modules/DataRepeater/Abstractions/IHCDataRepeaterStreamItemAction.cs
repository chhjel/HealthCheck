using HealthCheck.Core.Modules.DataRepeater.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.DataRepeater.Abstractions
{
    /// <summary>
    /// An action that can be performed on an item.
    /// </summary>
    public interface IHCDataRepeaterStreamItemAction
    {
        /// <summary>
        /// Id of the stream this item belongs to.
        /// </summary>
        string StreamId { get; }

        /// <summary>
        /// Type of the parameters object passed to <see cref="ExecuteActionAsync"/>.
        /// <para>Can be null if no parameters are used.</para>
        /// </summary>
        Type ParametersType { get; }

        /// <summary>
        /// Name to show in the UI.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Description to show in the UI.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Label on button to show in the UI.
        /// </summary>
        string ExecuteButtonLabel { get; }

        /// <summary>
        /// If any tags are set here, then only items with those tags will have this action available.
        /// </summary>
        List<string> AllowedOnItemsWithTags { get; }

        /// <summary>
        /// Perform the action on the given item with the given parameters.
        /// </summary>
        Task<HCDataRepeaterStreamItemActionResult> ExecuteActionAsync(IHCDataRepeaterStreamItem item, object parameters);
    }
}
