using HealthCheck.Module.EndpointControl.Abstractions;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;

namespace HealthCheck.DevTest._TestImplementation.EndpointControl
{
    public class CustomBlockedJsonResult : IEndpointControlBlockedRequestResult
    {
        public string Id => "CustomBlockedJsonResult";

        public string Name => "Json";
        public string Description => "Returns a json error response.";
        public Type CustomPropertiesModelType => typeof(CustomBlockedJsonResultProperties);

        public ActionResult CreateBlockedMvcResult(ActionExecutingContext filterContext, object customProperties)
        {
            var properties = customProperties as CustomBlockedJsonResultProperties;

            filterContext.HttpContext.Response.StatusCode = properties.StatusCode;
            return new ContentResult
            {
                ContentType = "application/json",
                ContentEncoding = Encoding.UTF8,
                Content = $"{{ \"success\":{properties.Success.ToString().ToLower()}, \"errorMessage\":\"{HttpUtility.HtmlAttributeEncode(properties.ErrorMessage)}\" }}"
            };
        }

        public HttpResponseMessage CreateBlockedWebApiResult(HttpActionContext actionContext, object customProperties)
        {
            var properties = customProperties as CustomBlockedJsonResultProperties;

            return actionContext.Request.CreateResponse(
                (HttpStatusCode)properties.StatusCode,
                new { success = properties.Success, errorMessage = properties.ErrorMessage },
                actionContext.ControllerContext.Configuration.Formatters.JsonFormatter
            );
        }
    }

    public class CustomBlockedJsonResultProperties
    {
        public int StatusCode { get; set; } = 409;
        public string ErrorMessage { get; set; } = "Oh no!";
        public bool Success { get; set; } = false;
    }
}
