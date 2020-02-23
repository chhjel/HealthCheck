using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Dataflow
{
    /// <summary>
    /// Default implementation of <see cref="IDataflowService{TAccessRole}"/>.
    /// <para>Allows for multiple streams to be registered in the options object.</para>
    /// </summary>
    public class DefaultDataflowService<TAccessRole> : IDataflowService<TAccessRole>
    {
        private DefaultDataflowServiceOptions<TAccessRole> Options { get; }

        /// <summary>
        /// Default implementation of <see cref="IDataflowService{TAccessRole}"/>.
        /// <para>Allows for multiple streams to be registered in the options object.</para>
        /// </summary>
        public DefaultDataflowService(DefaultDataflowServiceOptions<TAccessRole> options)
        {
            Options = options;
        }

        /// <summary>
        /// Get metadata for all the available streams.
        /// </summary>
        public List<DataflowStreamMetadata<TAccessRole>> GetStreamMetadata()
        {
            return (Options.Streams ?? Enumerable.Empty<IDataflowStream<TAccessRole>>())
                .Where(x => x.IsVisible?.Invoke() != false)
                .Select(x => new DataflowStreamMetadata<TAccessRole>
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    SupportsFilterByDate = x.SupportsFilterByDate,
                    PropertyDisplayInfo = x.GetEntryPropertiesInfo()?.ToList() ?? new List<DataFlowPropertyDisplayInfo>(),
                    RolesWithAccess = x.RolesWithAccess
                })
                .ToList();
        }

        /// <summary>
        /// Get filtered entries from the given stream.
        /// </summary>
        public async Task<IEnumerable<IDataflowEntry>> GetEntries(string streamId, DataflowStreamFilter filter)
        {
            var stream = Options.Streams?.FirstOrDefault(x => x.Id == streamId);
            if (stream == null || stream.IsVisible?.Invoke() == false)
            {
                return Enumerable.Empty<IDataflowEntry>();
            }

            return await stream.GetLatestStreamEntriesAsync(filter);
        }
    }
}
