#if NETFULL
using HealthCheck.Module.EndpointControl.Abstractions;
using HealthCheck.Module.EndpointControl.Models;
using HealthCheck.Module.EndpointControl.Utils;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;

namespace HealthCheck.Module.EndpointControl.Results
{
    /// <summary>
    /// Forwards a copy of the result without blocking the request.
    /// </summary>
    public class EndpointControlForwardedRequestResult : IEndpointControlRequestResult
    {
        /// <inheritdoc />
        public string Id => "ForwardedRequestResult";

        /// <inheritdoc />
        public string Name => "Forward request";

        /// <inheritdoc />
        public string Description => "Forwards a copy of the request async without blocking it.";

        /// <inheritdoc />
        public bool CountAsBlockedRequest => false;

        /// <inheritdoc />
        public Type CustomPropertiesModelType => typeof(EndpointControlForwardedRequestResultProperties);

        /// <inheritdoc />
        public virtual EndpointControlRequestResultMvc CreateMvcResult(ActionExecutingContext filterContext, object customProperties)
        {
            var properties = customProperties as EndpointControlForwardedRequestResultProperties;
            Task.Run(() => ForwardMvcRequestAsync(filterContext.RequestContext.HttpContext.Request, properties));
            return new EndpointControlRequestResultMvc { UseBuiltInBlock = false };
        }

        /// <inheritdoc />
        public virtual EndpointControlRequestResultWebApi CreateWebApiResult(HttpActionContext actionContext, object customProperties)
        {
            var properties = customProperties as EndpointControlForwardedRequestResultProperties;
            Task.Run(() => ForwardWebApiRequestAsync(actionContext.Request, properties));
            return new EndpointControlRequestResultWebApi { UseBuiltInBlock = false };
        }

        /// <summary>
        /// Options model for this result type.
        /// </summary>
        public class EndpointControlForwardedRequestResultProperties
        {
            /// <summary>
            /// Where the requests will be forwarded to.
            /// </summary>
            public string TargetUrl { get; set; }

            /// <summary>
            /// If true, request paths will be appended to the target url.
            /// </summary>
            public bool AppendRelativeUrl { get; set; } = true;
        }

        /// <summary>
        /// Performs the forwarding.
        /// </summary>
        protected virtual async Task ForwardWebApiRequestAsync(HttpRequestMessage request, EndpointControlForwardedRequestResultProperties options)
        {
            if (request == null || options == null)
            {
                return;
            }

            try
            {
                var rawUrl = RequestUtils.GetUrl(request);
                var url = CreateForwardUrl(rawUrl, options);
                byte[] data = null;
                if (request.Method.Method != "GET")
                {
                    using var stream = new MemoryStream();
                    var context = (HttpContextBase)request.Properties["MS_HttpContext"];
                    context.Request.InputStream.Seek(0, SeekOrigin.Begin);
                    context.Request.InputStream.CopyTo(stream);
                    data = stream.ToArray();
                }
                await ForwardRequest(request.Method.Method, null, request, options, url, data);
            }
            catch (Exception ex)
            {
                OnForwardWebApiRequestFailed(request, options, ex);
            }
        }

        /// <summary>
        /// If an exception is thrown during <see cref="ForwardWebApiRequestAsync"/> this is called.
        /// </summary>
        protected virtual void OnForwardWebApiRequestFailed(HttpRequestMessage request, EndpointControlForwardedRequestResultProperties options, Exception ex) {}

        /// <summary>
        /// Performs the forwarding.
        /// </summary>
        protected virtual async Task ForwardMvcRequestAsync(HttpRequestBase request, EndpointControlForwardedRequestResultProperties options)
        {
            if (request == null || options == null)
            {
                return;
            }

            try
            {
                var rawUrl = RequestUtils.GetUrl(request);
                var url = CreateForwardUrl(rawUrl, options);
                var data = (request.HttpMethod != "GET") ? RequestUtils.ReadRequestBody(request.InputStream) : null;
                await ForwardRequest(request.HttpMethod, request, null, options, url, data);
            }
            catch (Exception ex)
            {
                OnForwardMvcRequestAsyncFailed(request, options, ex);
            }
        }

        /// <summary>
        /// If an exception is thrown during <see cref="ForwardMvcRequestAsync"/> this is called.
        /// </summary>
        protected virtual void OnForwardMvcRequestAsyncFailed(HttpRequestBase request, EndpointControlForwardedRequestResultProperties options, Exception ex) { }

        private async Task ForwardRequest(string method,
            HttpRequestBase mvcRequest, HttpRequestMessage apiRequest,
            EndpointControlForwardedRequestResultProperties options, string url, byte[] data)
        {
            var client = new WebClient();
            ConfigureWebClientInternal(client, mvcRequest, apiRequest, options);

            if (method == "GET")
            {
                await client.DownloadStringTaskAsync(url);
            }
            else
            {
                await client.UploadDataTaskAsync(url, method, data);
            }
        }

        /// <summary>
        /// Create url to forward request to.
        /// </summary>
        protected virtual string CreateForwardUrl(string rawUrl, EndpointControlForwardedRequestResultProperties options)
        {
            if (!options.AppendRelativeUrl)
            {
                return options.TargetUrl;
            }

            var targetUri = new UriBuilder(options.TargetUrl);
            var builder = new UriBuilder(rawUrl)
            {
                Host = targetUri.Host,
                Scheme = targetUri.Scheme
            };
            if (targetUri.Port != 80 && targetUri.Port != 443)
            {
                builder.Port = targetUri.Port;
            }
            return builder.Uri.ToString();
        }

        private void ConfigureWebClientInternal(WebClient client,
            HttpRequestBase mvcRequest,
            HttpRequestMessage apiRequest,
            EndpointControlForwardedRequestResultProperties options)
        {
            if (mvcRequest != null)
            {
                ConfigureMvcWebClient(client, mvcRequest, options);
            }
            else if (apiRequest != null)
            {
                ConfigureWebApiWebClient(client, apiRequest, options);
            }
        }

        /// <summary>
        /// Perform any custom configuration on the webclient that forwards the request here.
        /// </summary>
        protected virtual void ConfigureMvcWebClient(WebClient client, HttpRequestBase request, EndpointControlForwardedRequestResultProperties options) { }

        /// <summary>
        /// Perform any custom configuration on the webclient that forwards the request here.
        /// </summary>
        protected virtual void ConfigureWebApiWebClient(WebClient client, HttpRequestMessage request, EndpointControlForwardedRequestResultProperties options) { }
    }
}
#endif
