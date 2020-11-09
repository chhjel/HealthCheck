using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Messages.Abstractions
{
    /// <summary>
    /// An item to be shown in the messages module.
    /// </summary>
    public interface IHCMessageItem
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

        // Later:
        // - files
        // - custom implementation and just reflect w/ prop type + attribute ui hints
    }
}
