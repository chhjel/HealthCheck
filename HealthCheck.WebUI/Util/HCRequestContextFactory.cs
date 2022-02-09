﻿using HealthCheck.Core.Models;
using System;
using System.Collections.Generic;
using HealthCheck.Core.Extensions;

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

            model.HasRequestContext = request != null;
            model.RequestExecutionStartTime = context?.Timestamp ?? DateTimeOffset.Now;
            model.Method = request?.HttpMethod;
            model.Url = request?.Url?.ToString();
            model.Headers = request?.Headers?.AllKeys?.ToDictionaryIgnoreDuplicates(t => t, t => request.Headers[t]) ?? new Dictionary<string, string>();
            model.Cookies = request?.Cookies?.AllKeys?.ToDictionaryIgnoreDuplicates(t => t, t => request.Cookies[t].Value) ?? new Dictionary<string, string>();
            HCRequestContext.RequestItemGetter = (key) => HttpContext.Current?.Request?.RequestContext?.HttpContext?.Items?[key];
            HCRequestContext.RequestItemSetter = (key, val) =>
            {
                var req = HttpContext.Current?.Request;
                if (req?.RequestContext?.HttpContext?.Items != null)
                {
                    req.RequestContext.HttpContext.Items[key] = val;
                }
            };
#endif

#if NETCORE
            var context = HCIoCUtils.GetInstance<IHttpContextAccessor>()?.HttpContext;
            var request = context?.Request;
            
            model.HasRequestContext = request != null;
            model.RequestExecutionStartTime = DateTimeOffset.Now; // todo
            model.Method = request?.Method;
            model.Url = request?.GetDisplayUrl();
            model.Headers = request?.Headers.Keys?.ToDictionaryIgnoreDuplicates(t => t, t => request.Headers[t].ToString()) ?? new Dictionary<string, string>();
            model.Cookies = request?.Cookies.Keys?.ToDictionaryIgnoreDuplicates(t => t, t => request.Cookies[t]) ?? new Dictionary<string, string>();
            HCRequestContext.RequestItemGetter = (key) => HCIoCUtils.GetInstance<IHttpContextAccessor>()?.HttpContext?.Request?.HttpContext?.Items?[key];
            HCRequestContext.RequestItemSetter = (key, val) =>
            {
                var req = HCIoCUtils.GetInstance<IHttpContextAccessor>()?.HttpContext?.Request;
                if (req?.HttpContext?.Items != null)
                {
                    req.HttpContext.Items[key] = val;
                }
            };
#endif

            return model;
        }
    }
}
