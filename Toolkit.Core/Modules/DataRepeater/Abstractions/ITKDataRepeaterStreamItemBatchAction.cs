using QoDL.Toolkit.Core.Modules.DataRepeater.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;

/// <summary>
/// An action that can be performed on all items in a stream.
/// </summary>
public interface ITKDataRepeaterStreamItemBatchAction
{
    /// <summary>
    /// Type of the parameters object passed to <see cref="ExecuteBatchActionAsync"/>.
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
    /// <para>Must be a flags enum of the same type as the one used on the toolkit controller.</para>
    /// </summary>
    object AllowedAccessRoles { get; set; }

    /// <summary>
    /// Optional categories this action belongs to.
    /// <para>Can be used for more granular access configuration.</para>
    /// </summary>
    List<string> Categories { get; }

    /// <summary>
    /// Perform an action on the given stream items with the given parameters.
    /// </summary>
    /// <param name="item">Current item to process.</param>
    /// <param name="parameters">Parameters from frontend.</param>
    /// <param name="batchResult">Total result so far.</param>
    Task<TKDataRepeaterStreamItemBatchActionResult> ExecuteBatchActionAsync(ITKDataRepeaterStreamItem item, object parameters, TKDataRepeaterStreamBatchActionResult batchResult);
}
