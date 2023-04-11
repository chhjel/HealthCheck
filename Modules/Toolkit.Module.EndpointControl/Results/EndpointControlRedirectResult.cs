#if NETFULL || NETCORE
using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Module.EndpointControl.Abstractions;
using QoDL.Toolkit.Module.EndpointControl.Models;
using System;
using QoDL.Toolkit.Core.Models;
using System.Text;
#if NETCORE
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
#endif
#if NETFULL
using System.Web.Http.Controllers;
using System.Web.Mvc;
using System.Net.Http;
using System.Net;
#endif

namespace QoDL.Toolkit.Module.EndpointControl.Results
{
    /// <summary>
    /// Returns a redirect.
    /// </summary>
    public class EndpointControlRedirectResult : IEndpointControlRequestResult
    {
        /// <inheritdoc />
        public string Id => "RedirectResult";

        /// <inheritdoc />
        public string Name => "Redirect";

        /// <inheritdoc />
        public string Description => "Responds with a redirect.";

        /// <inheritdoc />
        public bool CountAsBlockedRequest => true;

        /// <inheritdoc />
        public Type CustomPropertiesModelType => typeof(EndpointControlRedirectResultProperties);

#if NETCORE
        /// <inheritdoc />
        public virtual EndpointControlRequestResultMvc CreateMvcResult(ActionExecutingContext filterContext, object customProperties)
        {
            var properties = customProperties as EndpointControlRedirectResultProperties;
            var result = new EndpointControlRequestResultMvc
            {
                Result = new RedirectResult(properties.RedirectTarget, false)
            };
            return result;
        }
#endif

#if NETFULL
        /// <inheritdoc />
        public virtual EndpointControlRequestResultMvc CreateMvcResult(ActionExecutingContext filterContext, object customProperties)
        {
            var properties = customProperties as EndpointControlRedirectResultProperties;
            var result = new EndpointControlRequestResultMvc
            {
                Result = new RedirectResult(properties.RedirectTarget, false)
            };
            return result;
        }

        /// <inheritdoc />
        public virtual EndpointControlRequestResultWebApi CreateWebApiResult(HttpActionContext actionContext, object customProperties)
        {
            var properties = customProperties as EndpointControlRedirectResultProperties;
            var redirectResult = actionContext.Request.CreateResponse(HttpStatusCode.Redirect);
            redirectResult.Headers.Location = new Uri(properties.RedirectTarget);
            var result = new EndpointControlRequestResultWebApi
            {
                Result = redirectResult
            };
            return result;
        }
#endif

        /// <summary>
        /// Options model for this result type.
        /// </summary>
        public class EndpointControlRedirectResultProperties
        {
            /// <summary>
            /// Target location to redirect to.
            /// </summary>
            [TKCustomProperty(Description = "Url where the request will be redirected.", UIHints = TKUIHint.NotNull)]
            public string RedirectTarget { get; set; }
        }
    }
}
#endif
