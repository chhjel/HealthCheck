#if NETFULL
using QoDL.Toolkit.Module.EndpointControl.Abstractions;
using QoDL.Toolkit.Module.EndpointControl.Models;
using QoDL.Toolkit.Module.EndpointControl.Utils;
using System;
using System.Net;
using System.Text;
using System.Web.Mvc;

namespace QoDL.Toolkit.Module.EndpointControl.Attributes
{
    /// <summary>
    /// Allows for controlling execution of a given endpoint.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class TKControlledEndpointAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Allows for controlling execution of a given endpoint.
        /// </summary>
        /// <param name="name">Optional name of this endpoint.</param>
        public TKControlledEndpointAttribute(string name = null)
        {
            Name = name;
        }

        /// <summary>
        /// Optional name of this endpoint.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// If set to true, the request wont be blocked automatically.
        /// <para>You will have to handle the blocked status manually in the action by checking <see cref="EndpointControlUtils.CurrentRequestWasDecidedBlocked"/>.</para>
        /// </summary>
        public bool CustomBlockedHandling { get; set; }

        /// <summary>
        /// If set to true, requests wont be stored and counted for limit checks automatically.
        /// <para>Call <see cref="EndpointControlUtils.CountCurrentRequest"/> manually where you want the request to be counted/stored.</para>
        /// <para>Enable to allow a larger number of requests for e.g. validation, and only count the requests when input is valid.</para>
        /// </summary>
        public bool ManuallyCounted { get; set; }

        /// <summary>
        /// Defaults to 409 - Conflict.
        /// </summary>
        public HttpStatusCode BlockedStatusCode { get; set; } = HttpStatusCode.Conflict;

        /// <summary>
        /// Check if the request is allowed and block it if not.
        /// </summary>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var handledResult = AllowExecute(filterContext);
            var allowExecute = handledResult.WasDecidedToAllowRequest;
            if (!allowExecute && !CustomBlockedHandling)
            {
                var response = CreateBlockedResult(filterContext, handledResult);
                if (response != null)
                {
                    filterContext.Result = response;
                }
            }
        }
        
        /// <summary>
        /// Return false to deny execution.
        /// </summary>
        protected virtual EndpointControlHandledRequestResult AllowExecute(ActionExecutingContext filterContext)
        {
            var requestData = GetEndpointRequestData(filterContext);

            var handledResult = EndpointControlServices.EndpointControlService?.HandleRequest(requestData, storeData: !ManuallyCounted);
            var allowed = handledResult?.WasDecidedToAllowRequest != false;

            if (filterContext?.RequestContext?.HttpContext?.Items != null)
            {
                filterContext.RequestContext.HttpContext.Items[EndpointControlUtils.RequestItemKey_Allowed] = allowed;

                if (ManuallyCounted)
                {
                    filterContext.RequestContext.HttpContext.Items[EndpointControlUtils.RequestItemKey_RequestData] = requestData;
                }
            }

            return handledResult ?? new EndpointControlHandledRequestResult
            {
                WasDecidedToAllowRequest = true
            };
        }

        /// <summary>
        /// Result returned when execution is blocked.
        /// <para>Defaults to <c>"Too many requests. Try again later."</c> with status 409 for GET</para>
        /// <para>Defaults to <c>{ success = false, errorMessage = "Too many requests. Try again later." }</c> with status 409 for any other methods.</para>
        /// </summary>
        protected virtual ActionResult CreateBlockedResult(ActionExecutingContext filterContext, EndpointControlHandledRequestResult handledResult)
        {
            var customResult = handledResult?.CustomBlockedResult?.CreateMvcResult(filterContext, handledResult?.CreateCustomResultProperties());
            if (customResult?.Result != null)
            {
                return customResult.Result;
            }
            else if (customResult?.UseBuiltInBlock == false)
            {
                return null;
            }

            filterContext.HttpContext.Response.StatusCode = (int)BlockedStatusCode;
            if (filterContext.HttpContext.Request.HttpMethod == "GET")
            {
                return CreateDefaultBlockedResponse_Get(filterContext);
            }
            else
            {
                return CreateDefaultBlockedResponse_NotGet(filterContext);
            }
        }

        /// <summary>
        /// Create default blocked response for GET requests used by default <see cref="CreateBlockedResult"/> method.
        /// </summary>
        protected virtual ActionResult CreateDefaultBlockedResponse_Get(ActionExecutingContext filterContext)
        {
            filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;
            return new ContentResult { Content = "Too many requests. Try again later." };
        }

        /// <summary>
        /// Create default blocked response for any non-GET requests used by default <see cref="CreateBlockedResult"/> method.
        /// </summary>
        protected virtual ActionResult CreateDefaultBlockedResponse_NotGet(ActionExecutingContext filterContext)
        {
            filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;
            return new ContentResult
            {
                ContentType = "application/json",
                ContentEncoding = Encoding.UTF8,
                Content = "{\"success\":false,\"errorMessage\":\"Too many requests. Try again later.\"}"
            };
        }

        /// <summary>
        /// Gather information about the request to send to <see cref="IEndpointControlService"/>.
        /// </summary>
        protected virtual EndpointControlEndpointRequestData GetEndpointRequestData(ActionExecutingContext filterContext)
        {
            var data = new EndpointControlEndpointRequestData
            {
                ControllerType = filterContext?.ActionDescriptor?.ControllerDescriptor?.ControllerType,
                ControllerName = filterContext?.ActionDescriptor?.ControllerDescriptor?.ControllerName,
                ActionName = filterContext?.ActionDescriptor?.ActionName,

                UserLocationId = RequestUtils.GetIPAddress(filterContext?.HttpContext?.Request),
                UserAgent = filterContext?.HttpContext?.Request?.UserAgent,
                HttpMethod = filterContext?.HttpContext?.Request?.HttpMethod,
                Url = RequestUtils.GetUrl(filterContext?.HttpContext?.Request),

                HttpContext = filterContext?.HttpContext
            };

            data.EndpointName = Name ?? $"{data.ControllerName?.Replace("Controller", "")}.{data.ActionName} ({data.HttpMethod})";
            data.EndpointId = $"{data.ControllerName?.Replace("Controller", "")}|{data.ActionName}|{data.HttpMethod}";

            return data;
        }
    }
}
#endif
