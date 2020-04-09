using HealthCheck.Core.Modules.EventNotifications;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.DevTest._TestImplementation.EventNotifier
{
    public class MyNotifier : IEventNotifier
    {
        public string Id => "my_notifier";
        public string Name => "My Notifier";
        public string Description => "Does nothing, just an example.";
        public Func<bool> IsEnabled { get; set; } = () => true;
        public HashSet<string> PlaceholdersWithOnlyNames => null;
        public Dictionary<string, Func<string>> Placeholders { get; } = new Dictionary<string, Func<string>>
        {
            { "Custom_Placeholder", () => "Custom placeholder replaced successfully." }
        };

        private const string OPTION_MESSAGE = "message";
        private const string OPTION_TIMEOUT = "timeout";

        public IEnumerable<EventNotifierOptionDefinition> Options => new[]
        {
            new EventNotifierOptionDefinition(
                id: OPTION_MESSAGE,
                name: "Message",
                description: "Text that will be outputted."
            ),
            new EventNotifierOptionDefinition(OPTION_TIMEOUT, "Timeout in seconds"),
        };

        public async Task<string> NotifyEvent(NotifierConfig notifierConfig, string eventId, Dictionary<string, string> payloadValues)
        {
            // Placeholders will be replaced when calling GetOption()
            var message = notifierConfig.GetOption(OPTION_MESSAGE);

            try
            {
                Console.WriteLine(message);

                // The latest 10 returned strings will be stored and displayed in the UI.
                return await Task.FromResult($"Message '{message}' was outputted.");
            }
            catch (Exception ex)
            {
                return $"Failed to create message '{message}'. {ex.Message}";
            }
        }
    }
}
