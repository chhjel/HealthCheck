using System.Collections.Generic;

namespace HealthCheck.Module.EndpointControl.Models
{
    internal class LatestEndpointRequestsHistory
    {
        public Dictionary<string, LatestUserEndpointRequestHistory> IdentityRequests { get; set; }
            = new Dictionary<string, LatestUserEndpointRequestHistory>();
        public List<string> LatestRequestIdentities { get; set; } = new List<string>();
    }
}
