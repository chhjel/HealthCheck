using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Messages.Abstractions
{
    /// <summary>
    /// An item to be shown in the messages module.
    /// </summary>
    public interface ITKMessageItem
    {
        /// <summary>
        /// Unique id of the message.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// When the message was sent.
        /// </summary>
        DateTimeOffset Timestamp { get; }

        /// <summary>
        /// Summary of the message to show in the message list.
        /// </summary>
        string Summary { get; }

        /// <summary>
        /// Who the message was sent from.
        /// </summary>
        string From { get; }

        /// <summary>
        /// Who the message was sent to.
        /// </summary>
        string To { get; }

        /// <summary>
        /// Main body of the message.
        /// </summary>
        string Body { get; }

        /// <summary>
        /// True if the body should be rendered as HTML.
        /// </summary>
        bool BodyIsHtml { get; }

        /// <summary>
        /// Any additional details.
        /// </summary>
        Dictionary<string, string> AdditionalDetails { get; }

        /// <summary>
        /// True if the message failed to send.
        /// </summary>
        bool HasError { get; }

        /// <summary>
        /// Details about any error that occured during attempted sending of the message.
        /// </summary>
        string ErrorMessage { get; }
    }
}
