/* Todo
#if NETCORE
using HealthCheck.Core.Attributes;
using HealthCheck.Core.Extensions;
using HealthCheck.Module.EndpointControl.Abstractions;
using HealthCheck.Module.EndpointControl.Models;
using HealthCheck.Module.EndpointControl.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
        public string Description => "Forwards a copy of the request async.";

        /// <inheritdoc />
        public bool CountAsBlockedRequest => false;

        /// <inheritdoc />
        public Type CustomPropertiesModelType => typeof(EndpointControlForwardedRequestResultProperties);

        /// <inheritdoc />
        public virtual EndpointControlRequestResultMvc CreateMvcResult(ActionExecutingContext filterContext, object customProperties)
        {
            var properties = customProperties as EndpointControlForwardedRequestResultProperties;
            Task.Run(() => ForwardMvcRequestAsync(filterContext.HttpContext.Request, properties));

            var result = new EndpointControlRequestResultMvc { UseBuiltInBlock = false };
            if (properties?.ReplaceOriginalResponse == true)
            {
                result.Result = new ContentResult()
                {
                    Content = "",
                    ContentType = "text/plain",
                    //ContentEncoding = Encoding.UTF8
                };
            }
            return result;
        }

        /// <summary>
        /// Options model for this result type.
        /// </summary>
        public class EndpointControlForwardedRequestResultProperties
        {
            /// <summary>
            /// Where the requests will be forwarded to.
            /// </summary>
            [HCCustomProperty(Description = "Location where the requests will be forwarded.")]
            public string TargetUrl { get; set; }

            /// <summary>
            /// If true, request paths will be appended to the target url.
            /// </summary>
            [HCCustomProperty(Description = "If enabled only the host of the target url will be used, and the path of requests will be appended.")]
            public bool KeepRelativeUrls { get; set; }

            /// <summary>
            /// If true, header values will be forwarded as well.
            /// </summary>
            [HCCustomProperty(Description = "Enable to forward most headers as well.")]
            public bool IncludeHeaderValues { get; set; }

            /// <summary>
            /// If true, the original response will be replaced with an empty 200 OK response.
            /// </summary>
            [HCCustomProperty(Description = "If enabled, the original response will be replaced with an empty 200 OK response.")]
            public bool ReplaceOriginalResponse { get; set; }
        }

        /// <summary>
        /// Performs the forwarding.
        /// </summary>
        protected virtual async Task ForwardMvcRequestAsync(HttpRequest request, EndpointControlForwardedRequestResultProperties options)
        {
            if (request == null || options == null)
            {
                return;
            }

            try
            {
                var rawUrl = RequestUtils.GetUrl(request);
                var url = CreateForwardUrl(rawUrl, options);
                var data = (request.Method != "GET") ? RequestUtils.ReadRequestBody(request.Body) : null;
                await ForwardRequest(request.Method, request, options, url, data);
            }
            catch (Exception ex)
            {
                OnForwardMvcRequestAsyncFailed(request, options, ex);
            }
        }

        /// <summary>
        /// If an exception is thrown during <see cref="ForwardMvcRequestAsync"/> this is called.
        /// </summary>
        protected virtual void OnForwardMvcRequestAsyncFailed(HttpRequest request, EndpointControlForwardedRequestResultProperties options, Exception ex) { }

        private async Task ForwardRequest(string method,
            HttpRequest mvcRequest,
            EndpointControlForwardedRequestResultProperties options, string url, byte[] data)
        {
            var client = new WebClient();
            ConfigureWebClientInternal(client, mvcRequest, options);

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
            if (!options.KeepRelativeUrls)
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
            HttpRequest mvcRequest,
            EndpointControlForwardedRequestResultProperties options)
        {
            var requestDetails = new RequestDetails();

            if (mvcRequest != null)
            {
                requestDetails.RawUrl = RequestUtils.GetUrl(mvcRequest);
                requestDetails.Headers = mvcRequest.Headers.Keys.ToDictionaryIgnoreDuplicates(x => x, x => mvcRequest.Headers[x].FirstOrDefault());
                ConfigureMvcWebClient(client, mvcRequest, options);
            }

            ConfigureWebClient(client, requestDetails, options);
        }

        /// <summary>
        /// Perform any custom configuration on the webclient that forwards MVC requests here.
        /// </summary>
        protected virtual void ConfigureMvcWebClient(WebClient client, HttpRequest request, EndpointControlForwardedRequestResultProperties options) { }

        /// <summary>
        /// Perform any custom configuration on the webclient for both Mvc and WebApi here.
        /// </summary>
        protected virtual void ConfigureWebClient(WebClient client, RequestDetails details, EndpointControlForwardedRequestResultProperties options)
        {
            if (options.IncludeHeaderValues && details?.Headers?.Any() == true)
            {
                foreach (var header in details.Headers.Where(x => includeHeader(x)))
                {
                    client.Headers.Set(header.Key, header.Value);
                }
            }

            static bool includeHeader(KeyValuePair<string, string> kvp)
            {
                return !_exemptHeaders.Any(h => h.ToLower() == kvp.Key.ToLower());
            }
        }
        private static readonly string[] _exemptHeaders = new[] {
            "Connection",
            "Host",
            "Content-Length"
        };

        /// <summary>
        /// Shared details for MVC and WebApi requests.
        /// </summary>
        public class RequestDetails
        {
            /// <summary>
            /// All header values in the request.
            /// </summary>
            public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

            /// <summary>
            /// Raw incoming url.
            /// </summary>
            public string RawUrl { get; set; }
        }
    }
}
#endif
*/
