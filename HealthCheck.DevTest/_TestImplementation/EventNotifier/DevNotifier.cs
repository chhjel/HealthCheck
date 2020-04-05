using HealthCheck.Core.Modules.EventNotifications;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace HealthCheck.DevTest._TestImplementation.EventNotifier
{
    public class DevNotifier : IEventNotifier
    {
        public string Id => "dev";
        public string Name => "Dev Notifier";
        public string Description => "etc";
        public Func<bool> IsEnabled { get; set; } = () => true;
        public IEnumerable<EventNotifierOptionDefinition> Options => new[]
        {
            new EventNotifierOptionDefinition(OPTION_URL, "Url", "Placeholders can be used."),
            new EventNotifierOptionDefinition(OPTION_A, "Another field"),
            new EventNotifierOptionDefinition(OPTION_B, "Test", "Something here"),
        };
        private const string OPTION_URL = "url";
        private const string OPTION_A = "a";
        private const string OPTION_B = "b";

        public async Task<string> NotifyEvent(NotifierConfig notifierConfig, string eventId, Dictionary<string, string> payloadValues, Func<string, string> templateResolver)
        {
            var url = notifierConfig.GetOption(OPTION_URL);
            url += $"?{notifierConfig.GetOption(OPTION_A)}&{notifierConfig.GetOption(OPTION_B)}";

            try
            {
                foreach (var kvp in payloadValues)
                {
                    var value = HttpUtility.UrlEncode(kvp.Value ?? "");
                    url = url.Replace($"{{{kvp.Key?.ToUpper()}}}", value);
                }

                return await Task.FromResult($"Sent GET request to '{url}'");
            }
            catch (Exception ex)
            {
                return $"Failed to send GET request to '{url}'. {ex.Message}";
            }
        }
    }
}
