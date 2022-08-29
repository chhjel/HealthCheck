using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.Comparison.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Comparison.Abstractions
{
    /// <summary>
    /// Handles instance selection and resolving.
    /// </summary>
    public abstract class HCComparisonTypeHandlerBase<TContent> : IHCComparisonTypeHandler
        where TContent: class
    {
        /// <inheritdoc />
        public virtual Type ContentType => typeof(TContent);

        /// <inheritdoc />
        public virtual string Name => ContentType.Name.SpacifySentence();

        /// <inheritdoc />
        public abstract string Description { get; }

        /// <inheritdoc />
        public virtual string FindInstanceSearchPlaceholder { get; }

        /// <inheritdoc />
        public virtual string FindInstanceDescription { get; }

        /// <inheritdoc />
        public abstract Task<List<HCComparisonInstanceSelection>> GetFilteredOptionsAsync(HCComparisonTypeFilter filter);

        /// <inheritdoc />
        public string GetInstanceDisplayName(object instance)
            => GetInstanceDisplayNameOf(instance as TContent);

        /// <inheritdoc />
        public virtual async Task<object> GetInstanceWithIdAsync(string id)
            => await GetInstanceWithIdOfAsync(id);

        /// <summary>
        /// Get an instance of a <typeparamref name="TContent"/> with the given id. The id is one that was returned from <see cref="GetFilteredOptionsAsync"/>.
        /// </summary>
        public abstract Task<TContent> GetInstanceWithIdOfAsync(string id);

        /// <inheritdoc />
        public abstract string GetInstanceDisplayNameOf(TContent instance);
    }
}
