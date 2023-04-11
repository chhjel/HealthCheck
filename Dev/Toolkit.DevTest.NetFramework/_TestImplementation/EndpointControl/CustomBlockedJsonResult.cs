using QoDL.Toolkit.Module.EndpointControl.Abstractions;
using QoDL.Toolkit.Module.EndpointControl.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;

namespace QoDL.Toolkit.DevTest._TestImplementation.EndpointControl
{
    public class CustomBlockedJsonResult : IEndpointControlRequestResult
    {
        public string Id => "CustomBlockedJsonResult";

        public string Name => "Json";
        public string Description => "Returns a json error response.";
        public bool CountAsBlockedRequest => true;
        public Type CustomPropertiesModelType => typeof(CustomBlockedJsonResultProperties);

        public EndpointControlRequestResultMvc CreateMvcResult(ActionExecutingContext filterContext, object customProperties)
        {
            var properties = customProperties as CustomBlockedJsonResultProperties;

            filterContext.HttpContext.Response.StatusCode = properties.StatusCode;
            var result = new ContentResult
            {
                ContentType = "application/json",
                ContentEncoding = Encoding.UTF8,
                Content = $"{{ \"success\":{properties.Success.ToString().ToLower()}, \"errorMessage\":\"{HttpUtility.HtmlAttributeEncode(properties.ErrorMessage)}\" }}"
            };
            return new EndpointControlRequestResultMvc(result);
        }

        public EndpointControlRequestResultWebApi CreateWebApiResult(HttpActionContext actionContext, object customProperties)
        {
            var properties = customProperties as CustomBlockedJsonResultProperties;

            var result = actionContext.Request.CreateResponse(
                (HttpStatusCode)properties.StatusCode,
                new { success = properties.Success, errorMessage = properties.ErrorMessage },
                actionContext.ControllerContext.Configuration.Formatters.JsonFormatter
            );
            return new EndpointControlRequestResultWebApi(result);
        }
    }

    public class CustomBlockedJsonResultProperties
    {
        public int StatusCode { get; set; } = 409;
        public string ErrorMessage { get; set; } = "Oh no!";
        public bool Success { get; set; } = true;
        public DateTime Date { get; set; }
        public bool? NullBool { get; set; }
        public DateTimeOffset? DateOffset { get; set; }
    }
}
