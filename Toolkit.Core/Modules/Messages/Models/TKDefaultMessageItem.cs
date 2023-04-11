using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Core.Modules.Messages.Abstractions;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Messages.Models
{
    /// <summary>
    /// Default message item implementation.
    /// </summary>
    public class TKDefaultMessageItem : ITKMessageItem
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
        /// Any additional notes.
        /// </summary>
        public List<string> Notes { get; set; } = new List<string>();

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
        public TKDefaultMessageItem()
        {
            Id = Guid.NewGuid().ToString();
            Timestamp = DateTimeOffset.Now;
        }

        /// <summary>
        /// Default message item implementation.
        /// <para>Summary will be created from body.</para>
        /// </summary>
        public TKDefaultMessageItem(string from, string to, string body, bool isHtml)
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
        public TKDefaultMessageItem(string summary, string from, string to, string body, bool isHtml)
            : this(from, to, body, isHtml)
        {
            Summary = summary;
        }

        /// <summary>
        /// Add any additional detail.
        /// </summary>
        public TKDefaultMessageItem SetDetail(string name, string value)
        {
            AdditionalDetails[name] = value;
            return this;
        }

        /// <summary>
        /// Add any additional note about this email.
        /// </summary>
        public TKDefaultMessageItem AddNote(string note)
        {
            Notes.Add(note);
            return this;
        }

        /// <summary>
        /// Add an error to this message.
        /// </summary>
        public TKDefaultMessageItem SetError(string errorMessage)
        {
            ErrorMessage = errorMessage;
            return this;
        }

        /// <summary>
        /// Add an error to this message from the given exception.
        /// </summary>
        public TKDefaultMessageItem SetError(Exception exception)
        {
            ErrorMessage = (exception == null) ? null : TKExceptionUtils.GetFullExceptionDetails(exception);
            return this;
        }
    }
}
