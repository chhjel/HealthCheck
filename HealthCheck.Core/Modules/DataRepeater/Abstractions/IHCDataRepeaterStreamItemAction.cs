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
        /// Optional access roles that can execute this action.
        /// <para>Must be a flags enum of the same type as the one used on the healthcheck controller.</para>
        /// </summary>
        object AllowedAccessRoles { get; set; }

        /// <summary>
        /// Optional categories this action belongs to.
        /// <para>Can be used for more granular access configuration.</para>
        /// </summary>
        List<string> Categories { get; }

        /// <summary>
        /// Checks if the action is allowed to run on the given item.
        /// <para>Will be executed and checked before <see cref="ExecuteActionAsync"/> is run.</para>
        /// </summary>
        Task<HCDataRepeaterStreamItemActionAllowedResult> ActionIsAllowedForAsync(IHCDataRepeaterStreamItem item);

        /// <summary>
        /// Perform the action on the given item with the given parameters.
        /// </summary>
        Task<HCDataRepeaterStreamItemActionResult> ExecuteActionAsync(IHCDataRepeaterStreamItem item, object parameters);
    }
}
