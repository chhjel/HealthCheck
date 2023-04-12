using System.Collections.Generic;

namespace QoDL.Toolkit.Module.EndpointControl.Models;

/// <summary>
/// Model used by built in implementations to store request history.
/// </summary>
public class LatestUserEndpointRequestHistory
{
    /// <summary>
    /// E.g. client IP.
    /// </summary>
    public string UserLocationIdentifier { get; set; }

    /// <summary>
    /// Total requests for this IP.
    /// </summary>
    public long TotalRequestCount { get; set; }

    /// <summary>
    /// Latest requests for this IP.
    /// </summary>
    public Queue<EndpointRequestDetails> LatestRequests { get; set; } = new Queue<EndpointRequestDetails>();
}
