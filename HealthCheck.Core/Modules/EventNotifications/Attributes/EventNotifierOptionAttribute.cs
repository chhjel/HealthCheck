using System;

namespace HealthCheck.Core.Modules.EventNotifications.Attributes
{
    /// <summary>
    /// Defines a property on a notifier options model as a option.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class EventNotifierOptionAttribute : Attribute
    {
        /// <summary>
        /// Name to display in the UI.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Optional description to display in the UI.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// True if placeholders should be replaced. Placeholders are only supported on string types.
        /// <para>Defaults to true.</para>
        /// </summary>
        public bool ReplacePlaceholders { get; set; } = true;

        /// <summary>
        /// Optionally provide the name of a public static method in the same class that placeholder values will pass through.
        /// <para>The method must return a string, and have a single string parameter.</para>
        /// </summary>
        public string PlaceholderTransformerMethod { get; set; }
        
        /// <summary>
        /// Hint of how to display a parameter input.
        /// </summary>
        public UIHint UIHints { get; set; }

        /// <summary>
        /// Hint of how to display a parameter input.
        /// </summary>
        [Flags]
        public enum UIHint
        {
            /// <summary>
            /// Default.
            /// </summary>
            None = 0,

            /// <summary>
            /// Only affects strings. Shows a multi-line text area instead of a small input field.
            /// </summary>
            TextArea = 1,
        }

        /// <summary>
        /// Defines a property on a notifier options model as a option.
        /// </summary>
        /// <param name="name">Name to display in the UI. Defaults to prettified property name.</param>
        /// <param name="description">Optional description to display in the UI.</param>
        /// <param name="replacePlaceholders">If any placeholders should be replaced.</param>
        /// <param name="placeholderTransformerMethod">
        /// Optionally provide the name of a public static method in the same class that placeholder values will pass through.
        /// <para>The method must return the same type as this property, and have a single parameter of the same type as well.</para>
        /// </param>
        /// <param name="uiHints">Any hints of how to display this option.</param>
        public EventNotifierOptionAttribute(string name = null, string description = null,
            bool replacePlaceholders = true, string placeholderTransformerMethod = null,
            UIHint uiHints = UIHint.None)
        {
            Name = name;
            Description = description;
            ReplacePlaceholders = replacePlaceholders;
            PlaceholderTransformerMethod = placeholderTransformerMethod;
            UIHints = uiHints;
        }
    }
}
