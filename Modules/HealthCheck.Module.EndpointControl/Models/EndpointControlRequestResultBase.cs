using HealthCheck.Module.EndpointControl.Abstractions;

namespace HealthCheck.Module.EndpointControl.Models
{
    /// <summary>
    /// Result from <see cref="IEndpointControlRequestResult"/>.
    /// </summary>
    public class EndpointControlRequestResultBase
    {
        /// <summary>
        /// If set to true, the default block logic will be used if no other result is returned.
        /// <para>Set to false to not block the request.</para>
        /// </summary>
        public bool UseBuiltInBlock { get; set; }
    }
}
