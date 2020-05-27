using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.EventNotifications.Abstractions;
using HealthCheck.Core.Modules.EventNotifications.Attributes;
using HealthCheck.Core.Modules.EventNotifications.Models;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Core.Modules.EventNotifications.Models
{
    /// <summary>
    /// Model used when retrieving event notification configs.
    /// </summary>
    public class GetEventNotificationConfigsViewModel
    {
        /// <summary>
        /// Available notifiers.
        /// </summary>
        public IEnumerable<EventNotifierViewModel> Notifiers { get; set; } = Enumerable.Empty<EventNotifierViewModel>();

        /// <summary>
        /// All defined configs.
        /// </summary>
        public IEnumerable<EventSinkNotificationConfig> Configs { get; set; }
        
        /// <summary>
        /// All known event definitions.
        /// </summary>
        public IEnumerable<KnownEventDefinition> KnownEventDefinitions { get; set; }

        /// <summary>
        /// All placeholders that should work for all notifier options.
        /// </summary>
        public IEnumerable<string> Placeholders { get; set; }
    }

    /// <summary></summary>
    public class EventNotifierViewModel
    {
        /// <summary></summary>
        public string Id { get; }
        /// <summary></summary>
        public string Name { get; }
        /// <summary></summary>
        public string Description { get; }
        /// <summary></summary>
        public IEnumerable<EventNotifierOptionDefinition> Options { get; }
        /// <summary></summary>
        public IEnumerable<string> Placeholders { get; }

        /// <summary>
        /// Create view model.
        /// </summary>
        public EventNotifierViewModel(IEventNotifier notifier)
        {
            Id = notifier.Id;
            Name = notifier.Name;
            Description = notifier.Description;
            Options = CreateOptions(notifier);
            Placeholders =
                (notifier.PlaceholdersWithOnlyNames?.ToList() ?? Enumerable.Empty<string>())
                .Union((notifier.Placeholders == null) ? Enumerable.Empty<string>() : notifier.Placeholders.Keys.ToList());
        }

        internal IEnumerable<EventNotifierOptionDefinition> CreateOptions(IEventNotifier notifier)
        {
            var optionsType = notifier.OptionsModelType;
            if (optionsType == null) return Enumerable.Empty<EventNotifierOptionDefinition>();

            var options = optionsType.GetProperties()
                .Select(x => new
                {
                    Attribute = x.GetCustomAttributes(typeof(EventNotifierOptionAttribute), true).FirstOrDefault() as EventNotifierOptionAttribute,
                    Property = x
                })
                .Where(x => x.Attribute != null);

            var optionDefs = new List<EventNotifierOptionDefinition>();
            foreach (var option in options)
            {
                var id = option.Property.Name;
                var name = option.Attribute.Name;
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = option.Property.Name.SpacifySentence();
                }

                var def = new EventNotifierOptionDefinition(id, name, option.Attribute.Description)
                {
                    SupportsPlaceholders = option.Attribute.ReplacePlaceholders && option.Property.PropertyType == typeof(string),
                    Type = option.Property.PropertyType.Name,
                    UIHints = option.Attribute.UIHints
                };

                optionDefs.Add(def);
            }
            return optionDefs;
        }
    }
}
