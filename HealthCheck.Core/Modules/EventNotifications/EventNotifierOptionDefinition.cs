﻿namespace HealthCheck.Core.Modules.EventNotifications
{
    /// <summary>
    /// A custom config for this notifier.
    /// <para>Will be displayed as a input field.</para>
    /// </summary>
    public class EventNotifierOptionDefinition
    {
        /// <summary>
        /// Unique id of this property. Will be used as key in <see cref="NotifierConfig.Options"/>.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name to display in the UI.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Optional description to display in the UI.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A custom config for this notifier.
        /// <para>Will be displayed as a input field.</para>
        /// </summary>
        /// <param name="id">Unique id of this property. Will be used as key in <see cref="NotifierConfig.Options"/>.</param>
        /// <param name="name">Name to display in the UI.</param>
        /// <param name="description">Optional description to display in the UI.</param>
        public EventNotifierOptionDefinition(string id, string name, string description = null)
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }
}