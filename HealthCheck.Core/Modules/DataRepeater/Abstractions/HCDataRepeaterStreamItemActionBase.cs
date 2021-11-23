using HealthCheck.Core.Modules.DataRepeater.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.DataRepeater.Abstractions
{
    /// <summary>
    /// An action that can be performed on an item.
    /// <para>Base class with stronger typing.</para>
    /// </summary>
    public abstract class HCDataRepeaterStreamItemActionBase<TParameters> : IHCDataRepeaterStreamItemAction
        where TParameters: class, new()
    {
        /// <inheritdoc />
        public string StreamId { get; }

        /// <inheritdoc />
        public Type ParametersType => typeof(TParameters);

        /// <inheritdoc />
        public abstract string DisplayName { get; }

        /// <inheritdoc />
        public abstract string Description { get; }

        /// <inheritdoc />
        public abstract string ExecuteButtonLabel { get; }

        /// <inheritdoc />
        public abstract List<string> AllowedOnItemsWithTags { get; }

        /// <inheritdoc />
        public virtual object AllowedAccessRoles { get; set; }

        /// <inheritdoc />
        public virtual List<string> Categories { get; } = new();

        /// <inheritdoc />
        public virtual async Task<HCDataRepeaterStreamItemActionResult> ExecuteActionAsync(IHCDataRepeaterStreamItem item, object parameters)
        {
            return await PerformActionAsync(item, parameters as TParameters);
        }

        /// <summary>
        /// Perform the action on the given item with the given parameters.
        /// </summary>
        protected abstract Task<HCDataRepeaterStreamItemActionResult> PerformActionAsync(IHCDataRepeaterStreamItem item, TParameters parameters);
    }
}
