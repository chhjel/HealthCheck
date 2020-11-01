using System;

namespace HealthCheck.Module.EndpointControl.Models
{
    internal class EndpointRequestDetails
	{
		public string UserLocationIdentifier { get; set; }
		public string EndpointId { get; set; }
		public DateTimeOffset Timestamp { get; set; }
		public string UserAgent { get; set; }
		public string Url { get; set; }
		public bool WasBlocked { get; set; }
	}
}
