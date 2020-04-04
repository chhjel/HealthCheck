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
        public string Id => "__WebHookNotifier";

        /// <summary>
        /// Name of this notifier to display in the user interface.
        /// </summary>
        public string Name => "WebHook";

        /// <summary>
        /// Optional description of this notifier to display in the user interface.
        /// </summary>
        public string Description => "Sends a GET request to the given url.";

        /// <summary>
        /// Is this notifier enabled?
        /// </summary>
        public Func<bool> IsEnabled { get; set; } = () => true;

        /// <summary>
        /// Options for this notifier.
        /// </summary>
        public IEnumerable<EventNotifierOptionDefinition> Options => new[]
        {
            new EventNotifierOptionDefinition(OPTION_URL, "Url", "Placeholders can be used.")
        };
        
        private const string OPTION_URL = "url";

        /// <summary>Sends a GET request.</summary>
        public async Task<string> NotifyEvent(NotifierConfig notifierConfig, string eventId, Dictionary<string, string> payloadValues, Func<string, string> templateResolver)
        {
            var url = notifierConfig.GetOption(OPTION_URL);

            try
            {
                foreach(var kvp in payloadValues)
                {
                    var value = HttpUtility.UrlEncode(kvp.Value ?? "");
                    url = url.Replace($"{{{kvp.Key?.ToUpper()}}}", value);
                }

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
    }
}
