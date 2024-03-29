#if NETFULL
using QoDL.Toolkit.Module.EndpointControl.Abstractions;
using System.Net.Http;

namespace QoDL.Toolkit.Module.EndpointControl.Models;

/// <summary>
/// WebApi result from <see cref="IEndpointControlRequestResult"/>.
/// </summary>
public class EndpointControlRequestResultWebApi : EndpointControlRequestResultBase
{
    /// <summary>
    /// Result to return if any.
    /// </summary>
    public HttpResponseMessage Result { get; set; }

    /// <summary>
    /// WebApi result from <see cref="IEndpointControlRequestResult"/>.
    /// </summary>
    public EndpointControlRequestResultWebApi(HttpResponseMessage result = null)
    {
        Result = result;
    }
}
#endif
