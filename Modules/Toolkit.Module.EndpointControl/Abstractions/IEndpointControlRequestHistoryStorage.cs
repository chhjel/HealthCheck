using QoDL.Toolkit.Module.EndpointControl.Models;
using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Module.EndpointControl.Abstractions;

/// <summary>
/// Stores request history.
/// </summary>
public interface IEndpointControlRequestHistoryStorage
{
    /// <summary>
    /// Store data about a given request.
    /// </summary>
    void AddRequest(EndpointControlEndpointRequestData request);

    /// <summary>
    /// Get the number of requests from a given location id since the given time.
    /// </summary>
    long GetTotalRequestCountSince(string locationId, DateTimeOffset time);

    /// <summary>
    /// Get the number of requests from a given location id on a given endpoint since the given time.
    /// </summary>
    long GetEndpointRequestCountSince(string locationId, string endpointId, DateTimeOffset time);

    /// <summary>
    /// Get latest requests for all endpoints.
    /// </summary>
    IEnumerable<EndpointRequestDetails> GetLatestRequests(int maxCount);
}
