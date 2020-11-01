using System.Collections.Generic;

namespace HealthCheck.Module.EndpointControl.Models
{
    internal class LatestUserEndpointRequestHistory
	{
		public string UserLocationIdentifier { get; set; }
		public long TotalRequestCount { get; set; }
		public Queue<EndpointRequestDetails> LatestRequests { get; set; } = new Queue<EndpointRequestDetails>();
	}
}
