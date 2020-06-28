using HealthCheck.Core.Modules.EventNotifications.Abstractions;
using HealthCheck.Core.Modules.EventNotifications.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.DevTest._TestImplementation.EventNotifier
{
    public class SimpleNotifier : IEventNotifier
    {
        public string Id => "simple";
        public string Name => "Simple Notifier";
        public string Description => "Does nothing.";
        public Func<bool> IsEnabled { get; set; } = () => true;
        public HashSet<string> PlaceholdersWithOnlyNames => null;
        public Dictionary<string, Func<string>> Placeholders => null;
        public Type OptionsModelType => null;
        
        public async Task<string> NotifyEvent(NotifierConfig notifierConfig, string eventId, Dictionary<string, string> payloadValues, object optionsObject)
        {
            return await Task.FromResult<string>(null);
        }
    }
}
