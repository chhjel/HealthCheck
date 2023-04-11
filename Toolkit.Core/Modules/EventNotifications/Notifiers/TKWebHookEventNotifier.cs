using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Models;
using QoDL.Toolkit.Core.Modules.EventNotifications.Abstractions;
using QoDL.Toolkit.Core.Modules.EventNotifications.Attributes;
using QoDL.Toolkit.Core.Modules.EventNotifications.Models;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace QoDL.Toolkit.Core.Modules.EventNotifications.Notifiers
{
    /// <summary>
    /// Sends a GET request to the given url.
    /// </summary>
    public class TKWebHookEventNotifier : IEventNotifier
    {
        /// <summary>
        /// Unique id of this notifier.
        /// </summary>
        public virtual string Id => "__WebHookNotifier";

        /// <summary>
        /// Name of this notifier to display in the user interface.
        /// </summary>
        public virtual string Name => "WebHook";

        /// <summary>
        /// Optional description of this notifier to display in the user interface.
        /// </summary>
        public virtual string Description => "Sends a web request to the given url.";

        /// <summary>
        /// Is this notifier enabled?
        /// </summary>
        public virtual Func<bool> IsEnabled { get; set; } = () => true;

        /// <summary>
        /// No special placeholders for this one.
        /// </summary>
        public virtual Dictionary<string, Func<string>> Placeholders => new();

        /// <summary>
        /// No special placeholders for this one.
        /// </summary>
        public virtual HashSet<string> PlaceholdersWithOnlyNames => new();

        /// <summary>
        /// Provide the options model type.
        /// </summary>
        public Type OptionsModelType => typeof(WebHookEventNotifierOptions);

        /// <summary>Sends a GET request.</summary>
        public virtual async Task<string> NotifyEvent(NotifierConfig notifierConfig, string eventId, Dictionary<string, string> payloadValues, object optionsObject)
        {
            var options = optionsObject as WebHookEventNotifierOptions;
            var url = options.Url;

            try
            {
                using (var client = new WebClient())
                {
                    if (options.Method == WebHookEventNotifierMethod.POST)
                    {
                        if (!string.IsNullOrWhiteSpace(options.ContentType))
                        {
                            client.Headers["Content-Type"] = options.ContentType;
                        }
                        await client.UploadStringTaskAsync(url, options.PostBody);
                    }
                    else
                    {
                        await client.DownloadStringTaskAsync(url);
                    }
                }

                return $"Sent {options.Method} request to '{url}'";
            }
            catch (Exception ex)
            {
                return $"Failed to send {options.Method} request to '{url}'. {TKExceptionUtils.GetFullExceptionDetails(ex)}";
            }
        }

        /// <summary>
        /// Options model for <see cref="TKWebHookEventNotifier"/>.
        /// </summary>
        public class WebHookEventNotifierOptions
        {
            /// <summary>
            /// Where the request will be sent.
            /// </summary>
            [EventNotifierOption(Description = "Where the request will be sent.", PlaceholderTransformerMethod = nameof(UrlEncoder), UIHints = TKUIHint.NotNull)]
            public string Url { get; set; }

            /// <summary>
            /// What type of request method to use.
            /// </summary>
            [EventNotifierOption(Description = "Request method.", UIHints = TKUIHint.NotNull)]
            public WebHookEventNotifierMethod Method { get; set; } = WebHookEventNotifierMethod.GET;

            /// <summary>
            /// Content type of the request.
            /// </summary>
            [EventNotifierOption(Description = "Content-Type header used if not empty. Only relevant for POST requests.", UIHints = TKUIHint.NotNull)]
            public string ContentType { get; set; } = "application/json";

            /// <summary>
            /// Where the request will be sent.
            /// </summary>
            [EventNotifierOption(Description = "If request method is POST, this will be the body that is sent. Any placeholders will be json string encoded.",
                PlaceholderTransformerMethod = nameof(PostBodyEncoder),
                UIHints = TKUIHint.NotNull | TKUIHint.CodeArea)]
            public string PostBody { get; set; }

            /// <summary>
            /// Encodes any placeholders in the Url property.
            /// </summary>
            public static string UrlEncoder(string value) => HttpUtility.UrlEncode(value ?? "");

            /// <summary>
            /// Encodes any placeholders in the PostBody property.
            /// </summary>
            public static string PostBodyEncoder(string value) => TKGlobalConfig.Serializer?.Serialize(value, false) ?? value;
        }

        /// <summary>
        /// What method to use.
        /// </summary>
        public enum WebHookEventNotifierMethod
        {
            /// <summary></summary>
            GET,
            /// <summary></summary>
            POST
        }
    }
}
