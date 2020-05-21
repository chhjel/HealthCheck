using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Core.Modules.EventNotifications.Models
{
    /// <summary>
    /// A model for event sink notification configs.
    /// </summary>
    public class EventSinkNotificationConfig
    {
        /// <summary>
        /// Id of this config instance.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name of user that last changed this notifier.
        /// </summary>
        public string LastChangedBy { get; set; }

        /// <summary>
        /// Date when last changed.
        /// </summary>
        public DateTime LastChangedAt { get; set; }

        /// <summary>
        /// Date of latest notification.
        /// </summary>
        public DateTime? LastNotifiedAt { get; set; }

        /// <summary>
        /// Enable this config.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Optionally limit number of notifications before disabling it.
        /// </summary>
        public int? NotificationCountLimit { get; set; }

        /// <summary>
        /// Optionally limit notifications to after this time.
        /// </summary>
        public DateTime? FromTime { get; set; }

        /// <summary>
        /// Optionally limit notifications to before this time.
        /// </summary>
        public DateTime? ToTime { get; set; }

        /// <summary>
        /// Id of notifiers to notify when events match.
        /// </summary>
        public List<NotifierConfig> NotifierConfigs { get; set; }

        /// <summary>
        /// Filter for event event ids.
        /// </summary>
        public EventSinkNotificationConfigFilter EventIdFilter { get; set; }

        /// <summary>
        /// Filter for event payloads.
        /// </summary>
        public List<EventSinkNotificationConfigFilter> PayloadFilters { get; set; }

        /// <summary>
        /// 10 last notification results.
        /// </summary>
        public List<string> LatestResults { get; set; } = new List<string>();

        /// <summary>
        /// Returns true if all <see cref="PayloadFilters"/> match the input.
        /// </summary>
        public bool IsAllowed(string eventId, bool payloadIsComplex,
            string stringifiedPayload, Dictionary<string, string> payloadProperties)
        {
            if (LimitHasBeenReached())
            {
                return false;
            }
            else if (FromTime != null && DateTime.Now < FromTime)
            {
                return false;
            }
            else if (EventIdFilter?.IsAllowed(false, eventId, null) == false)
            {
                return false;
            }

            if (PayloadFilters == null || !PayloadFilters.Any())
            {
                return true;
            }

            return PayloadFilters?.All(x => x.IsAllowed(payloadIsComplex, stringifiedPayload, payloadProperties)) == true;
        }

        /// <summary>
        /// True if any defined limits has been reached.
        /// </summary>
        public bool LimitHasBeenReached()
        {
            if (NotificationCountLimit != null && NotificationCountLimit <= 0)
            {
                return true;
            }
            else if (ToTime != null && DateTime.Now > ToTime)
            {
                return true;
            }

            return false;
        }
    }
}
