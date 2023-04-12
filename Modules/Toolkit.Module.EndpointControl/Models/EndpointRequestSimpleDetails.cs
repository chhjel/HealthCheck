using System;

namespace QoDL.Toolkit.Module.EndpointControl.Models;

/// <summary>
/// Simple request data used for displaying some data in the UI.
/// </summary>
public class EndpointRequestSimpleDetails
	{
		/// <summary>
		/// When the request was stored.
		/// </summary>
		public DateTimeOffset Timestamp { get; set; }

		/// <summary>
		/// True if the request was blocked.
		/// </summary>
		public bool WasBlocked { get; set; }

		/// <summary>
		/// Id of the endpoint.
		/// </summary>
		public string EndpointId { get; set; }
	}
