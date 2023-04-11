namespace QoDL.Toolkit.Module.EndpointControl.Models
{
    /// <summary>
    /// Definition of an endpoint.
    /// </summary>
    public class EndpointControlEndpointDefinition
	{
		/// <summary>
		/// Unique id of the endpoint.
		/// </summary>
		public string EndpointId { get; set; }

		/// <summary>
		/// Target controller for the request.
		/// </summary>
		public string ControllerName { get; set; }

		/// <summary>
		/// Target action for the request.
		/// </summary>
		public string ActionName { get; set; }

		/// <summary>
		/// Http method of the request.
		/// </summary>
		public string HttpMethod { get; set; }
	}

}
