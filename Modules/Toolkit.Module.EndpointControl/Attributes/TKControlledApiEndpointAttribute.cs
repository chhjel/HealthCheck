#if NETFULL
using QoDL.Toolkit.Module.EndpointControl.Abstractions;
using QoDL.Toolkit.Module.EndpointControl.Models;
using QoDL.Toolkit.Module.EndpointControl.Utils;
using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace QoDL.Toolkit.Module.EndpointControl.Attributes
{
    /// <summary>
    /// Allows for controlling execution of a given webapi endpoint.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class TKControlledApiEndpointAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Allows for controlling execution of a given webapi endpoint.
        /// </summary>
        /// <param name="name">Optional name of this endpoint.</param>
        public TKControlledApiEndpointAttribute(string name = null)
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
        /// Check if the request is allowed and block it if not.
        /// </summary>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var handledResult = AllowExecute(actionContext);
            var allowExecute = handledResult.WasDecidedToAllowRequest;
            if (!allowExecute && !CustomBlockedHandling)
            {
                var response = CreateBlockedResult(actionContext, handledResult);
                if (response != null)
                {
                    actionContext.Response = response;
                }
            }
        }

        /// <summary>
        /// Return false to deny execution.
        /// </summary>
        protected virtual EndpointControlHandledRequestResult AllowExecute(HttpActionContext actionContext)
        {
            var requestData = GetEndpointRequestData(actionContext);
            
            var handledResult = EndpointControlServices.EndpointControlService?.HandleRequest(requestData, storeData: !ManuallyCounted);
            var allowed = handledResult?.WasDecidedToAllowRequest != false;

            if (HttpContext.Current?.Items != null)
            {
                HttpContext.Current.Items[EndpointControlUtils.RequestItemKey_Allowed] = allowed;

                if (ManuallyCounted)
                {
                    HttpContext.Current.Items[EndpointControlUtils.RequestItemKey_RequestData] = requestData;
                }
            }

            return handledResult ?? new EndpointControlHandledRequestResult
            {
                WasDecidedToAllowRequest = true
            };
        }

        /// <summary>
        /// Result returned when execution is blocked.
        /// <para>Defaults to <c>{ success = false, errorMessage = "Too many requests. Try again later." }</c> with status 409.</para>
        /// </summary>
        protected virtual HttpResponseMessage CreateBlockedResult(HttpActionContext actionContext, EndpointControlHandledRequestResult handledResult)
        {
            var customResult = handledResult?.CustomBlockedResult?.CreateWebApiResult(actionContext, handledResult?.CreateCustomResultProperties());
            if (customResult?.Result != null)
            {
                return customResult.Result;
            }
            else if (customResult?.UseBuiltInBlock == false)
            {
                return null;
            }

            return actionContext.Request.CreateResponse(
                HttpStatusCode.Conflict,
                new { success = false, errorMessage = "Too many requests. Try again later." },
                actionContext.ControllerContext.Configuration.Formatters.JsonFormatter
            );
        }

        /// <summary>
        /// Gather information about the request to send to <see cref="IEndpointControlService"/>.
        /// </summary>
        protected virtual EndpointControlEndpointRequestData GetEndpointRequestData(HttpActionContext actionContext)
        {
            var data = new EndpointControlEndpointRequestData
            {
                ControllerType = actionContext?.ActionDescriptor?.ControllerDescriptor?.ControllerType,
                ControllerName = actionContext?.ActionDescriptor?.ControllerDescriptor?.ControllerName,
                ActionName = actionContext?.ActionDescriptor?.ActionName,

                UserLocationId = RequestUtils.GetIPAddress(actionContext?.Request),
                UserAgent = actionContext?.Request?.Headers?.UserAgent?.ToString(),
                HttpMethod = actionContext?.Request?.Method?.ToString(),
                Url = RequestUtils.GetUrl(actionContext?.Request),
            };

            data.EndpointName = Name ?? $"{data.ControllerName?.Replace("Controller", "")}.{data.ActionName} ({data.HttpMethod})";
            data.EndpointId = $"{data.ControllerName?.Replace("Controller", "")}|{data.ActionName}|{data.HttpMethod}";

            return data;
        }
    }
}
#endif
