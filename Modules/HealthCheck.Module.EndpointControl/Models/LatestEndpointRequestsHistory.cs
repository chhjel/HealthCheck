using System.Collections.Generic;

namespace HealthCheck.Module.EndpointControl.Models
{
    internal class LatestEndpointRequestsHistory
    {
        public List<string> LatestRequestIdentities { get; set; } = new List<string>();

        public Dictionary<string, LatestUserEndpointRequestHistory> IdentityRequests { get; set; }
            = new Dictionary<string, LatestUserEndpointRequestHistory>();

        /// <summary>
        /// Latest requests across all endpoints and identities.
        /// </summary>
        public Queue<EndpointRequestDetails> LatestRequests { get; set; } = new Queue<EndpointRequestDetails>();
    }
}
