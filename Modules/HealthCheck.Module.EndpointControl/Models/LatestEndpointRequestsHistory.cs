using System.Collections.Generic;

namespace HealthCheck.Module.EndpointControl.Models
{
    /// <summary>
    /// Model used by built in implementations to store request history.
    /// </summary>
    public class LatestEndpointRequestsHistory
    {
        /// <summary></summary>
        public List<string> LatestRequestIdentities { get; set; } = new();

        /// <summary></summary>
        public Dictionary<string, LatestUserEndpointRequestHistory> IdentityRequests { get; set; } = new();

        /// <summary>
        /// Latest requests across all endpoints and identities.
        /// </summary>
        public Queue<EndpointRequestDetails> LatestRequests { get; set; } = new();
    }
}
