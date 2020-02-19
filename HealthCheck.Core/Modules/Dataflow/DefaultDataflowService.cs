using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Dataflow
{
    /// <summary>
    /// Default implementation of <see cref="IDataflowService"/>.
    /// <para>Allows for multiple streams to be registered in the options object.</para>
    /// </summary>
    public class DefaultDataflowService : IDataflowService
    {
        private DefaultDataflowServiceOptions Options { get; }

        /// <summary>
        /// Default implementation of <see cref="IDataflowService"/>.
        /// <para>Allows for multiple streams to be registered in the options object.</para>
        /// </summary>
        public DefaultDataflowService(DefaultDataflowServiceOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Get metadata for all the available streams.
        /// </summary>
        public List<DataflowStreamMetadata> GetStreamMetadata()
        {
            return (Options.Streams ?? Enumerable.Empty<IDataflowStream>())
                .Select(x => new DataflowStreamMetadata
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    SupportsFilterByDate = x.SupportsFilterByDate,
                    PropertyDisplayInfo = x.GetEntryPropertiesInfo()?.ToList() ?? new List<DataFlowPropertyDisplayInfo>()
                })
                .ToList();
        }

        /// <summary>
        /// Get filtered entries from the given stream.
        /// </summary>
        public async Task<IEnumerable<IDataflowEntry>> GetEntries(string streamId, DataflowStreamFilter filter)
        {
            var stream = Options.Streams?.FirstOrDefault(x => x.Id == streamId);
            if (stream == null)
            {
                return Enumerable.Empty<IDataflowEntry>();
            }

            return await stream.GetLatestStreamEntriesAsync(filter);
        }
    }
}
