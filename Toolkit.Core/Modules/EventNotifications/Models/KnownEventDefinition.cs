using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.EventNotifications.Models
{
    /// <summary>
    /// Auto-created definition of event data.
    /// </summary>
    public class KnownEventDefinition
    {
        /// <summary>
        /// Id of the event.
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        /// Name of proeprties on the payload if any.
        /// </summary>
        public List<string> PayloadProperties { get; set; } = new List<string>();

        /// <summary>
        /// True if there's no properties and matching should occur on the stringified payload.
        /// </summary>
        public bool IsStringified { get; set; }
    }
}
