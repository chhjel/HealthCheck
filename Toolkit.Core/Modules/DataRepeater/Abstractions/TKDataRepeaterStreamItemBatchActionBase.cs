using QoDL.Toolkit.Core.Modules.DataRepeater.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions
{
    /// <summary>
    /// An action that can be performed on all items in a stream.
    /// <para>Base class with stronger typing.</para>
    /// </summary>
    public abstract class TKDataRepeaterStreamItemBatchActionBase<TParameters> : ITKDataRepeaterStreamItemBatchAction
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
        public async Task<TKDataRepeaterStreamItemBatchActionResult> ExecuteBatchActionAsync(ITKDataRepeaterStreamItem item, object parameters, TKDataRepeaterStreamBatchActionResult batchResult)
            => await PerformBatchActionAsync(item, parameters as TParameters, batchResult);

        /// <summary>
        /// Perform a batch action on the given stream item with the given parameters.
        /// </summary>
        /// <param name="item">Current item to process.</param>
        /// <param name="parameters">Parameters from frontend.</param>
        /// <param name="batchResult">Total result so far.</param>
        protected abstract Task<TKDataRepeaterStreamItemBatchActionResult> PerformBatchActionAsync(ITKDataRepeaterStreamItem item, TParameters parameters, TKDataRepeaterStreamBatchActionResult batchResult);
    }
}
