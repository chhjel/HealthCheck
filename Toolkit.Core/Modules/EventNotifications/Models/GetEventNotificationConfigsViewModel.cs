using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Core.Modules.EventNotifications.Abstractions;
using QoDL.Toolkit.Core.Util.Models;
using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Core.Modules.EventNotifications.Models;

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
    public IEnumerable<TKBackendInputConfig> Options { get; }
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
        Options = TKCustomPropertyAttribute.CreateInputConfigs(notifier.OptionsModelType, (config, prop, attr) =>
        {
            if (prop?.PropertyType == typeof(string))
            {
                config.Flags.Add("SupportsPlaceholders");
            }
        });
        Placeholders =
            (notifier.PlaceholdersWithOnlyNames?.ToList() ?? Enumerable.Empty<string>())
            .Union((notifier.Placeholders == null) ? Enumerable.Empty<string>() : notifier.Placeholders.Keys.ToList());
    }
}
