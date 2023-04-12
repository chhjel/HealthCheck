using QoDL.Toolkit.Module.EndpointControl.Models;

namespace QoDL.Toolkit.Module.EndpointControl.Utils;

/// <summary></summary>
public class EndpointControlRequestHistoryStorageHelper
{
    /// <summary>
    /// The max latest number of identities to store in memory.
    /// </summary>
    public virtual int MaxMemoryIdentityCount { get; set; } = 1000;

    /// <summary>
    /// Max number of latest requests to store in memory per identity.
    /// </summary>
    public virtual int MaxMemoryRequestCountPerIdentity { get; set; } = 1000;

    /// <summary>
    /// Max number of latest requests to store in memory.
    /// </summary>
    public virtual int MaxMemoryLatestRequestCount { get; set; } = 500;

    /// <summary></summary>
    public LatestEndpointRequestsHistory Data = new();

    /// <summary></summary>
    public virtual bool AddRequestToCollections(EndpointControlEndpointRequestData request, LatestEndpointRequestsHistory data = null)
    {
        data ??= Data;
        if (string.IsNullOrWhiteSpace(request?.UserLocationId)) return false;

        lock (data.LatestRequests)
        {
            var details = CreateRequestDetails(request);
            data.LatestRequests.Enqueue(details);

            if (data.LatestRequests.Count > MaxMemoryLatestRequestCount)
            {
                data.LatestRequests.Dequeue();
            }
        }

        lock (data.IdentityRequests)
        {
            lock (data.LatestRequestIdentities)
            {
                // Append request if identity already exists in memory
                if (data.IdentityRequests.ContainsKey(request.UserLocationId))
                {
                    AddRequest(data.IdentityRequests[request.UserLocationId], request);

                    // Move identity to the top
                    var oldIndex = data.LatestRequestIdentities.IndexOf(request.UserLocationId);
                    (data.LatestRequestIdentities[oldIndex], data.LatestRequestIdentities[0]) = (data.LatestRequestIdentities[0], data.LatestRequestIdentities[oldIndex]);
                    return true;
                }

                // Create new if missing
                var newItem = new LatestUserEndpointRequestHistory()
                {
                    UserLocationIdentifier = request.UserLocationId
                };
                AddRequest(newItem, request);

                data.IdentityRequests[request.UserLocationId] = newItem;
                data.LatestRequestIdentities.Insert(0, request.UserLocationId);

                // Cleanup if needed
                if (data.LatestRequestIdentities.Count > MaxMemoryIdentityCount)
                {
                    var indexToRemove = data.LatestRequestIdentities.Count - 1;
                    var removedIdentity = data.LatestRequestIdentities[indexToRemove];
                    data.LatestRequestIdentities.RemoveAt(indexToRemove);
                    data.IdentityRequests.Remove(removedIdentity);
                }
            }
        }
        return true;
    }

    private void AddRequest(LatestUserEndpointRequestHistory container, EndpointControlEndpointRequestData request)
    {
        lock (container.LatestRequests)
        {
            container.TotalRequestCount++;

            var details = CreateRequestDetails(request);
            container.LatestRequests.Enqueue(details);

            if (container.LatestRequests.Count > MaxMemoryRequestCountPerIdentity)
            {
                container.LatestRequests.Dequeue();
            }
        }
    }

    private static EndpointRequestDetails CreateRequestDetails(EndpointControlEndpointRequestData request)
    {
        return new EndpointRequestDetails
        {
            UserLocationIdentifier = request.UserLocationId,
            EndpointId = request.EndpointId,
            Timestamp = request.Timestamp,
            Url = request.Url,
            UserAgent = request.UserAgent,
            WasBlocked = request.WasBlocked,
            BlockingRuleId = request.BlockingRuleId
        };
    }
}
