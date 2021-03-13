using System;
#if NETFULL
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Mvc;
#endif

namespace HealthCheck.Module.EndpointControl.Abstractions
{
    /// <summary>
    /// Defines a custom result that can be selected for blocked requests.
    /// </summary>
    public interface IEndpointControlBlockedRequestResult
    {
        /// <summary>
        /// Id of this action.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Name of this action to show in the UI.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Description of this action to show in the UI.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Model for custom properties if any to display in the UI.
        /// <para>Only string, int and bool properties supported for now.</para>
        /// </summary>
        Type CustomPropertiesModelType { get; }
        
#if NETFULL
        /// <summary>
        /// Used by controllers inheriting from <see cref="System.Web.Mvc.Controller"/>.
        /// </summary>
        ActionResult CreateBlockedMvcResult(ActionExecutingContext filterContext, object customProperties);

        /// <summary>
        /// Used by controllers inheriting from <see cref="System.Web.Http.ApiController"/>.
        /// </summary>
        HttpResponseMessage CreateBlockedWebApiResult(HttpActionContext actionContext, object customProperties);
#endif
    }
}
