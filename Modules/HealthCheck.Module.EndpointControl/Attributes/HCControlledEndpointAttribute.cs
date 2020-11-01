﻿#if NETFULL
using HealthCheck.Module.EndpointControl.Abstractions;
using HealthCheck.Module.EndpointControl.Models;
using HealthCheck.Module.EndpointControl.Utils;
using System;
using System.Net;
using System.Web.Mvc;

namespace HealthCheck.Module.EndpointControl.Attributes
{
    /// <summary>
    /// Allows for controlling execution of a given endpoint.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class HCControlledEndpointAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Allows for controlling execution of a given endpoint.
        /// </summary>
        /// <param name="name">Optional name of this endpoint.</param>
        public HCControlledEndpointAttribute(string name = null)
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
        /// Enable to allow a larger number of requests for e.g. validation, and only count the requests when input is valid.
        /// <para>ToDo: store data in items, get invoke store through EndpointControlUtils.</para>
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
            var allowExecute = AllowExecute(filterContext);
            if (!allowExecute && !CustomBlockedHandling)
            {
                filterContext.Result = CreateBlockedResult(filterContext);
                filterContext.HttpContext.Response.StatusCode = (int)BlockedStatusCode;
            }
        }

        /// <summary>
        /// Return false to deny execution.
        /// </summary>
        protected virtual bool AllowExecute(ActionExecutingContext filterContext)
        {
            var requestData = GetEndpointRequestData(filterContext);
            var allowed = EndpointControlServices.EndpointControlService?.HandleRequest(requestData) != false;
            filterContext.RequestContext.HttpContext.Items[EndpointControlUtils.RequestItemKey_Allowed] = allowed;
            return allowed;
        }

        /// <summary>
        /// Result returned when execution is blocked.
        /// <para>Defaults to <c>"Too many requests. Try again later."</c></para>
        /// </summary>
        protected virtual ActionResult CreateBlockedResult(ActionExecutingContext filterContext)
            => new ContentResult { Content = "Too many requests. Try again later." };

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

                UserLocationIdentifier = RequestUtils.GetIPAddress(filterContext?.HttpContext?.Request),
                UserAgent = filterContext?.HttpContext?.Request?.UserAgent,
                HttpMethod = filterContext?.HttpContext?.Request?.HttpMethod,
                Url = RequestUtils.GetUrl(filterContext?.HttpContext?.Request),
            };

            data.EndpointName = Name ?? $"{data.ControllerName?.Replace("Controller", "")}.{data.ActionName} ({data.HttpMethod})";

            return data;
        }

        ///// <summary>
        ///// Key to cache request data on. Defaults to {HttpMethod}-{ControllerName}-{ActionName}-{UserLocationId}
        ///// </summary>
        //protected virtual string CreateCacheKey(ActionExecutingContext filterContext, EndpointRequestData requestData)
        //{
        //    return string.Join(
        //        "-",
        //        requestData.HttpMethod,
        //        requestData.ControllerName,
        //        requestData.ActionName,
        //        requestData.UserLocationIdentifier
        //    );
        //}
    }
}
#endif
