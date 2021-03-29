using HealthCheck.Core.Attributes;

namespace HealthCheck.Core.Modules.EventNotifications.Attributes
{
    /// <summary>
    /// Defines a property on a notifier options model as a option.
    /// </summary>
    public class EventNotifierOptionAttribute : HCCustomPropertyAttribute
    {
        /// <summary>
        /// Optionally provide the name of a public static method in the same class that placeholder values will pass through.
        /// <para>The method must return a string, and have a single string parameter.</para>
        /// </summary>
        public string PlaceholderTransformerMethod { get; set; }
    }
}
