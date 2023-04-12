using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Module.EndpointControl.Abstractions;
using QoDL.Toolkit.Module.EndpointControl.Models;
using QoDL.Toolkit.Module.EndpointControl.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Module.EndpointControl.Storage;

/// <summary>
/// Stores request data in memory only.
/// </summary>
public class MemoryEndpointControlRequestHistoryStorage : IEndpointControlRequestHistoryStorage
{
    private readonly EndpointControlRequestHistoryStorageHelper _helper = new();
    private LatestEndpointRequestsHistory _data => _helper.Data;

    /// <inheritdoc />
    public void AddRequest(EndpointControlEndpointRequestData request)
    {
        _helper.AddRequestToCollections(request);
    }

    /// <inheritdoc />
    public long GetTotalRequestCountSince(string locationId, DateTimeOffset time)
    {
        lock (_data.IdentityRequests)
        {
            var queue = _data.IdentityRequests.ContainsKey(locationId) ? _data.IdentityRequests[locationId].LatestRequests : null;
            lock (queue)
            {
                return queue?.Count(x => x.Timestamp >= time) ?? 0;
            }
        }
    }

    /// <inheritdoc />
    public long GetEndpointRequestCountSince(string locationId, string endpointId, DateTimeOffset time)
    {
        lock (_data.IdentityRequests)
        {
            var queue = _data.IdentityRequests.ContainsKey(locationId) ? _data.IdentityRequests[locationId].LatestRequests : null;
            lock (queue)
            {
                return queue?.Count(x => x.EndpointId == endpointId && x.Timestamp >= time) ?? 0;
            }
        }
    }

    /// <inheritdoc />
    public IEnumerable<EndpointRequestDetails> GetLatestRequests(int maxCount)
    {
        lock (_data.LatestRequests)
        {
            return _data.LatestRequests.TakeLastN(maxCount).ToArray();
        }
    }
}
