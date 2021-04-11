using HealthCheck.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

#if NETFULL
using System.Web;
#endif

#if NETCORE
using HealthCheck.Core.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
#endif

namespace HealthCheck.WebUI.Util
{
    internal static class HCRequestContextFactory
    {
        public static HCRequestContext Create()
        {
            var model = new HCRequestContext();

#if NETFULL
            var context = HttpContext.Current;
            var request = context?.Request;

            model.RequestExecutionStartTime = context?.Timestamp ?? DateTime.Now;
            model.Method = request?.HttpMethod;
            model.Url = request?.Url?.ToString();
            model.Headers = request?.Headers?.AllKeys?.ToDictionary(t => t, t => request.Headers[t]) ?? new Dictionary<string, string>();
            model.Cookies = request?.Cookies?.AllKeys?.ToDictionary(t => t, t => request.Cookies[t].Value) ?? new Dictionary<string, string>();
#endif

#if NETCORE
            var context = IoCUtils.GetInstance<IHttpContextAccessor>()?.HttpContext;
            var request = context?.Request;

            model.RequestExecutionStartTime = DateTime.Now; // todo
            model.Method = request?.Method;
            model.Url = request?.GetDisplayUrl();
            model.Headers = request?.Headers.Keys?.ToDictionary(t => t, t => request.Headers[t].ToString()) ?? new Dictionary<string, string>();
            model.Cookies = request?.Cookies.Keys?.ToDictionary(t => t, t => request.Cookies[t]) ?? new Dictionary<string, string>();
#endif

            return model;
        }
    }
}
