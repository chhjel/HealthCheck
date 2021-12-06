using HealthCheck.Module.DataExport.Abstractions;
using HealthCheck.Module.DataExport.Models;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace HealthCheck.Module.DataExport.Services
{
    /// <summary>
    /// Handles export stream data.
    /// </summary>
    public class HCDefaultDataExportService : IHCDataExportService
    {
        private readonly IEnumerable<IHCDataExportStream> _streams;

        /// <summary>
        /// Handles export stream data.
        /// </summary>
        public HCDefaultDataExportService(IEnumerable<IHCDataExportStream> streams)
        {
            _streams = streams;
        }

        /// <inheritdoc />
        public IEnumerable<IHCDataExportStream> GetStreams() => _streams;

        /// <inheritdoc />
        public async Task<HCDataExportQueryResponse> QueryAsync(HCDataExportQueryRequest request)
        {
            var stream = GetStreamById(request);
            if (stream == null)
            {
                return new HCDataExportQueryResponse();
            }

            var queryable = await stream.GetQueryableAsync();
            var matches = queryable
                .Where(request.Query)
                .Skip(request.PageIndex * request.PageSize)
                .Take(request.PageSize)
                .ToArray();

            var result = new HCDataExportQueryResponse
            {
                Items = matches,
                TotalCount = queryable.Count()
            };
            return result;
        }

        private IHCDataExportStream GetStreamById(HCDataExportQueryRequest request)
            => _streams.FirstOrDefault(x => x.GetType().FullName == request.StreamId);
    }
}
