using HealthCheck.Core.Extensions;
using HealthCheck.Core.Models;
using HealthCheck.Core.Modules.EventNotifications.Abstractions;
using HealthCheck.Core.Modules.EventNotifications.Attributes;
using HealthCheck.Core.Modules.EventNotifications.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.EventNotifications.Notifiers
{
    /// <summary>
    /// Sends mail.
    /// </summary>
    public abstract class HCMailEventNotifierBase : IEventNotifier
    {
        #region IEventNotifier Implementation
        /// <inheritdoc />
        public virtual string Id => "mail";
        /// <inheritdoc />
        public virtual string Name => "E-Mail";
        /// <inheritdoc />
        public virtual string Description => "Sends email notifications.";
        /// <inheritdoc />
        public virtual Func<bool> IsEnabled => () => true;

        /// <inheritdoc />
        public virtual Dictionary<string, Func<string>> Placeholders => null;
        /// <inheritdoc />
        public virtual HashSet<string> PlaceholdersWithOnlyNames => null;
        /// <inheritdoc />
        public virtual Type OptionsModelType => typeof(MailEventNotifierOptions);
        #endregion

        /// <inheritdoc />
        public virtual async Task<string> NotifyEvent(NotifierConfig notifierConfig, string eventId,
            Dictionary<string, string> payloadValues, object optionsObject)
        {
            var options = optionsObject as MailEventNotifierOptions;
            var to = (options.To ?? "")
                .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .SelectMany(x => x.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            if (!to.Any())
            {
                return "No to-emails configured.";
            }

            var successMails = new List<string>();
            var failedMails = new List<string>();
            foreach (var toMail in to)
            {
                var success = await SendMailAsync(toMail, options.Subject, options.Body);
                if (success)
                {
                    successMails.Add(toMail);
                }
                else
                {
                    failedMails.Add(toMail);
                }
            }

            var result = "";
            if (successMails.Any())
            {
                result = $"Mail sent successfully to {successMails.JoinForSentence()}. ";
            }
            if (failedMails.Any())
            {
                result = $"Mail failed to send to {failedMails.JoinForSentence()}.";
            }

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Logic for sending the actual mail.
        /// </summary>
        protected abstract Task<bool> SendMailAsync(string toMail, string subject, string body);

        /// <summary>
        /// Options for <see cref="HCMailEventNotifierBase"/>.
        /// </summary>
        public class MailEventNotifierOptions
        {
            /// <summary>
            /// Who the mail is sent to.
            /// </summary>
            [EventNotifierOption(Name = "To e-mails", Description = "Addresses to send notifications to. Separated by ',' or ';'.")]
            public string To { get; set; }

            /// <summary>
            /// Subject of the mail.
            /// </summary>
            [EventNotifierOption(Description = "Subject of the e-mail.")]
            public string Subject { get; set; }

            /// <summary>
            /// Body of the mail.
            /// </summary>
            [EventNotifierOption(Description = "Contents of the e-mail.", UIHints = HCUIHint.TextArea)]
            public string Body { get; set; }
        }
    }
}
