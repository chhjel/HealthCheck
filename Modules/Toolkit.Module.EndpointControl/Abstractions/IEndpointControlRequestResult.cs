using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Module.EndpointControl.Models;
using System;
#if NETFULL
using System.Web.Http.Controllers;
using System.Web.Mvc;
#endif
#if NETCORE
using Microsoft.AspNetCore.Mvc.Filters;
#endif

namespace QoDL.Toolkit.Module.EndpointControl.Abstractions
{
    /// <summary>
    /// Defines a custom result that can be selected for requests.
    /// </summary>
    public interface IEndpointControlRequestResult
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
        /// If true, the request will be stored as blocked in historical data.
        /// </summary>
        bool CountAsBlockedRequest { get; }

        /// <summary>
        /// Model for custom properties if any to display in the UI.
        /// <para>Optionally decorate properties in the model with <see cref="TKCustomPropertyAttribute"/> to set descriptions etc.</para>
        /// </summary>
        Type CustomPropertiesModelType { get; }

#if NETFULL
        /// <summary>
        /// Used by controllers inheriting from <see cref="System.Web.Mvc.Controller"/>.
        /// </summary>
        EndpointControlRequestResultMvc CreateMvcResult(ActionExecutingContext filterContext, object customProperties);

        /// <summary>
        /// Used by controllers inheriting from <see cref="System.Web.Http.ApiController"/>.
        /// </summary>
        EndpointControlRequestResultWebApi CreateWebApiResult(HttpActionContext actionContext, object customProperties);
#endif

#if NETCORE
        /// <summary>
        /// Used by controllers inheriting from <see cref="Microsoft.AspNetCore.Mvc.Controller"/>.
        /// </summary>
        EndpointControlRequestResultMvc CreateMvcResult(ActionExecutingContext filterContext, object customProperties);
#endif
    }
}
