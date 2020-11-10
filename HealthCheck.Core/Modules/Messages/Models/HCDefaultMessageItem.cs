using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.Messages.Abstractions;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Messages.Models
{
    /// <summary>
    /// Default message item implementation.
    /// </summary>
    public class HCDefaultMessageItem : IHCMessageItem
    {
        /// <summary>
        /// Unique id of the message.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// When the message was sent.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Summary of the message to show in the message list.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Who the message was sent from.
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Who the message was sent to.
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// Main body of the message.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// True if the body should be rendered as HTML.
        /// </summary>
        public bool BodyIsHtml { get; set; }

        /// <summary>
        /// Any additional details.
        /// </summary>
        public Dictionary<string, string> AdditionalDetails { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// True if the message failed to send.
        /// </summary>
        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

        /// <summary>
        /// Details about any error that occured during attempted sending of the message.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Default message item implementation.
        /// </summary>
        public HCDefaultMessageItem()
        {
            Id = Guid.NewGuid().ToString();
            Timestamp = DateTimeOffset.Now;
        }

        /// <summary>
        /// Default message item implementation.
        /// <para>Summary will be created from body.</para>
        /// </summary>
        public HCDefaultMessageItem(string from, string to, string body, bool isHtml)
            : this()
        {
            From = from;
            To = to;
            Body = body;
            BodyIsHtml = isHtml;
            Summary = isHtml ? body.StripHtml().LimitMaxLength(30) : body.LimitMaxLength(30);
        }

        /// <summary>
        /// Default message item implementation.
        /// </summary>
        public HCDefaultMessageItem(string summary, string from, string to, string body, bool isHtml)
            : this(from, to, body, isHtml)
        {
            Summary = summary;
        }

        /// <summary>
        /// Add any additional detail.
        /// </summary>
        public HCDefaultMessageItem AddDetail(string name, string value)
        {
            AdditionalDetails[name] = value;
            return this;
        }

        /// <summary>
        /// Add an error to this message.
        /// </summary>
        public HCDefaultMessageItem SetError(string errorMessage)
        {
            ErrorMessage = errorMessage;
            return this;
        }

        /// <summary>
        /// Add an error to this message from the given exception.
        /// </summary>
        public HCDefaultMessageItem SetError(Exception exception)
        {
            ErrorMessage = (exception == null) ? null : ExceptionUtils.GetFullExceptionDetails(exception);
            return this;
        }
    }
}
