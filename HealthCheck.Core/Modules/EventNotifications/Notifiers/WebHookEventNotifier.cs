using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace HealthCheck.Core.Modules.EventNotifications.Notifiers
{
    /// <summary>
    /// Sends a GET request to the given url.
    /// </summary>
    public class WebHookEventNotifier : IEventNotifier
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
        public virtual string Description => "Sends a GET request to the given url.";

        /// <summary>
        /// Is this notifier enabled?
        /// </summary>
        public virtual Func<bool> IsEnabled { get; set; } = () => true;

        /// <summary>
        /// No special placeholders for this one.
        /// </summary>
        public virtual Dictionary<string, Func<string>> Placeholders => null;

        /// <summary>
        /// No special placeholders for this one.
        /// </summary>
        public virtual HashSet<string> PlaceholdersWithOnlyNames => null;

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
                    await client.DownloadStringTaskAsync(url);
                }

                return $"Sent GET request to '{url}'";
            }
            catch(Exception ex)
            {
                return $"Failed to send GET request to '{url}'. {ex.Message}";
            }
        }

        /// <summary>
        /// Options model for <see cref="WebHookEventNotifier"/>.
        /// </summary>
        public class WebHookEventNotifierOptions
        {
            /// <summary>
            /// Where the GET request will be sent.
            /// </summary>
            [EventNotifierOption(description: "Where the GET request will be sent.", placeholderTransformerMethod: nameof(UrlEncoder))]
            public string Url { get; set; }

            /// <summary>
            /// Encodes any placeholders in the Url property.
            /// </summary>
            public static string UrlEncoder(string value) => HttpUtility.UrlEncode(value ?? "");
        }
    }
}
