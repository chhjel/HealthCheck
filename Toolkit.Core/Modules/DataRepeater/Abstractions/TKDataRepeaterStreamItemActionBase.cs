using QoDL.Toolkit.Core.Modules.DataRepeater.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;

/// <summary>
/// An action that can be performed on an item.
/// <para>Base class with stronger typing.</para>
/// </summary>
public abstract class TKDataRepeaterStreamItemActionBase<TParameters> : ITKDataRepeaterStreamItemAction
    where TParameters : class, new()
{
    /// <inheritdoc />
    public Type ParametersType => typeof(TParameters);

    /// <inheritdoc />
    public abstract string DisplayName { get; }

    /// <inheritdoc />
    public abstract string Description { get; }

    /// <inheritdoc />
    public abstract string ExecuteButtonLabel { get; }

    /// <inheritdoc />
    public virtual object AllowedAccessRoles { get; set; }

    /// <inheritdoc />
    public virtual List<string> Categories { get; } = new();

    /// <inheritdoc />
    public virtual Task<TKDataRepeaterStreamItemActionAllowedResult> ActionIsAllowedForAsync(ITKDataRepeaterStreamItem item)
        => Task.FromResult(TKDataRepeaterStreamItemActionAllowedResult.CreateAllowed());

    /// <inheritdoc />
    public virtual async Task<TKDataRepeaterStreamItemActionResult> ExecuteActionAsync(ITKDataRepeaterStream stream, ITKDataRepeaterStreamItem item, object parameters)
    {
        return await PerformActionAsync(stream, item, parameters as TParameters);
    }

    /// <summary>
    /// Perform the action on the given item with the given parameters.
    /// </summary>
    protected abstract Task<TKDataRepeaterStreamItemActionResult> PerformActionAsync(ITKDataRepeaterStream stream, ITKDataRepeaterStreamItem item, TParameters parameters);
}
