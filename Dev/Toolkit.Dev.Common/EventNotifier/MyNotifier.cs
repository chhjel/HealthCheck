using QoDL.Toolkit.Core.Models;
using QoDL.Toolkit.Core.Modules.EventNotifications.Abstractions;
using QoDL.Toolkit.Core.Modules.EventNotifications.Attributes;
using QoDL.Toolkit.Core.Modules.EventNotifications.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.EventNotifier;

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

    public Type OptionsModelType => typeof(MyNotifierOptions);

    public async Task<string> NotifyEvent(NotifierConfig notifierConfig, string eventId, Dictionary<string, string> payloadValues, object optionsObject)
    {
        var options = optionsObject as MyNotifierOptions;
        var message = options.Message;

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

    public class MyNotifierOptions
    {
        [EventNotifierOption(Description = "Text that will be outputted, UIHints = TKUIHint.NotNull")]
        public string Message { get; set; }

        [EventNotifierOption(Description = "Timeout in seconds")]
        public int Timeout { get; set; }

        [EventNotifierOption(Description = "A test of TextArea UIHint", UIHints = TKUIHint.TextArea | TKUIHint.NotNull)]
        public string BodyTest { get; set; }

        [EventNotifierOption]
        public DateTimeOffset DateTest { get; set; }
    }
}
