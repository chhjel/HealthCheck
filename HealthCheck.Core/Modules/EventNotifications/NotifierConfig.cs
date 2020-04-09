using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.EventNotifications
{
    /// <summary>
    /// Option values for this notifier instance.
    /// </summary>
    public class NotifierConfig
    {
        /// <summary>
        /// Id of notifier to notify when events match.
        /// </summary>
        public string NotifierId { get; set; }

        /// <summary>
        /// Option values for this notifier instance.
        /// </summary>
        public Dictionary<string, string> Options { get; set; }

        internal Dictionary<string, string> Placeholders { get; set; }

        /// <summary>
        /// Get the option with the given id/key, or null if not present.
        /// </summary>
        public string GetOption(string id, bool resolvePlaceholders = true, Func<string, string> placeholderValueTransformer = null)
        {
            var value = (id != null && Options.ContainsKey(id)) ? Options[id] : null;
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            if ( resolvePlaceholders && Placeholders != null)
            {
                foreach (var kvp in Placeholders)
                {
                    var key = kvp.Key;
                    var placeholderValue = kvp.Value ?? "";

                    if (placeholderValueTransformer != null)
                    {
                        placeholderValue = placeholderValueTransformer.Invoke(placeholderValue) ?? "";
                    }

                    value = value.Replace($"{{{key?.ToUpper()}}}", placeholderValue);
                }
            }

            return value;
        }
    }
}
