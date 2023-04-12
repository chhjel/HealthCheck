using System;

namespace QoDL.Toolkit.Module.EndpointControl.Models;

	/// <summary>
	/// Historical request data.
	/// </summary>
public class EndpointRequestDetails
	{
		/// <summary>
		/// Usually IP or localhost.
		/// </summary>
		public string UserLocationIdentifier { get; set; }

		/// <summary>
		/// Id of the endpoint.
		/// </summary>
		public string EndpointId { get; set; }

		/// <summary>
		/// When the request was stored.
		/// </summary>
		public DateTimeOffset Timestamp { get; set; }

		/// <summary>
		/// Client user agent value.
		/// </summary>
		public string UserAgent { get; set; }

		/// <summary>
		/// Full url.
		/// </summary>
		public string Url { get; set; }

		/// <summary>
		/// True if the request was blocked.
		/// </summary>
		public bool WasBlocked { get; set; }

		/// <summary>
		/// Id of the blocking rule if any.
		/// </summary>
		public Guid? BlockingRuleId { get; set; }
	}
