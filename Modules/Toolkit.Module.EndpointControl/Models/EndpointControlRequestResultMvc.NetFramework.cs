#if NETFULL
using QoDL.Toolkit.Module.EndpointControl.Abstractions;
using System.Web.Mvc;

namespace QoDL.Toolkit.Module.EndpointControl.Models;

/// <summary>
/// MVC result from <see cref="IEndpointControlRequestResult"/>.
/// </summary>
public class EndpointControlRequestResultMvc : EndpointControlRequestResultBase
{
    /// <summary>
    /// Result to return if any.
    /// </summary>
    public ActionResult Result { get; set; }

    /// <summary>
    /// MVC result from <see cref="IEndpointControlRequestResult"/>.
    /// </summary>
    public EndpointControlRequestResultMvc(ActionResult result = null)
    {
        Result = result;
    }
}
#endif
