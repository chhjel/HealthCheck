#if NETFULL || NETCORE
using HealthCheck.Core.Attributes;
using HealthCheck.Module.EndpointControl.Abstractions;
using HealthCheck.Module.EndpointControl.Models;
using System;
using HealthCheck.Core.Models;
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

namespace HealthCheck.Module.EndpointControl.Results
{
    /// <summary>
    /// Returns customized content.
    /// </summary>
    public class EndpointControlContentResult : IEndpointControlRequestResult
    {
        /// <inheritdoc />
        public string Id => "ContentResult";

        /// <inheritdoc />
        public string Name => "Custom content";

        /// <inheritdoc />
        public string Description => "Responds with customizable content.";

        /// <inheritdoc />
        public bool CountAsBlockedRequest => true;

        /// <inheritdoc />
        public Type CustomPropertiesModelType => typeof(EndpointControlContentResultProperties);

#if NETCORE
        /// <inheritdoc />
        public virtual EndpointControlRequestResultMvc CreateMvcResult(ActionExecutingContext filterContext, object customProperties)
        {
            var properties = customProperties as EndpointControlContentResultProperties;
            var result = new EndpointControlRequestResultMvc
            {
                Result = new ContentResult()
                {
                    Content = properties.Content,
                    ContentType = properties.ContentType
                }
            };
            return result;
        }
#endif

#if NETFULL
        /// <inheritdoc />
        public virtual EndpointControlRequestResultMvc CreateMvcResult(ActionExecutingContext filterContext, object customProperties)
        {
            var properties = customProperties as EndpointControlContentResultProperties;
            var result = new EndpointControlRequestResultMvc
            {
                Result = new ContentResult()
                {
                    Content = properties.Content,
                    ContentType = properties.ContentType,
                    ContentEncoding = Encoding.UTF8
                }
            };
            return result;
        }

        /// <inheritdoc />
        public virtual EndpointControlRequestResultWebApi CreateWebApiResult(HttpActionContext actionContext, object customProperties)
        {
            var properties = customProperties as EndpointControlContentResultProperties;
            var contentResult = actionContext.Request.CreateResponse(HttpStatusCode.OK, properties.Content, properties.ContentType);
            var result = new EndpointControlRequestResultWebApi
            {
                Result = contentResult
            };
            return result;
        }
#endif

        /// <summary>
        /// Options model for this result type.
        /// </summary>
        public class EndpointControlContentResultProperties
        {
            /// <summary>
            /// The ContentType header.
            /// </summary>
            [HCCustomProperty(Description = "ContentType header to return.")]
            public string ContentType { get; set; } = "application/json";

            /// <summary>
            /// Content to return.
            /// </summary>
            [HCCustomProperty(Description = "The content to return.", UIHints = HCUIHint.NotNull | HCUIHint.CodeArea)]
            public string Content { get; set; } = "";
        }
    }
}
#endif
