using QoDL.Toolkit.Core.Abstractions.Modules;
using QoDL.Toolkit.Core.Modules.Messages.Abstractions;
using QoDL.Toolkit.Core.Modules.Messages.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Core.Modules.Messages;

/// <summary>
/// Module for viewing messages.
/// </summary>
public class TKMessagesModule : ToolkitModuleBase<TKMessagesModule.AccessOption>
{
    private TKMessagesModuleOptions Options { get; }

    /// <summary>
    /// Module for viewing messages.
    /// </summary>
    public TKMessagesModule(TKMessagesModuleOptions options)
    {
        Options = options;
    }

    /// <summary>
    /// Check options object for issues.
    /// </summary>
    public override IEnumerable<string> Validate()
    {
        var issues = new List<string>();
        if (Options.MessageStorage == null) issues.Add("Options.MessageStorage must be set.");
        return issues;
    }

    /// <summary>
    /// Get frontend options for this module.
    /// </summary>
    public override object GetFrontendOptionsObject(ToolkitModuleContext context) => null;

    /// <summary>
    /// Get config for this module.
    /// </summary>
    public override IToolkitModuleConfig GetModuleConfig(ToolkitModuleContext context) => new TKMessagesModuleConfig();
    
    /// <summary>
    /// Different access options for this module.
    /// </summary>
    [Flags]
    public enum AccessOption
    {
        /// <summary>Does nothing.</summary>
        None = 0,

        /// <summary>Allows for deleting messages.</summary>
        DeleteMessages = 1 << 1,
    }

    #region Invokable
    /// <summary>
    /// Delete a single message by id.
    /// </summary>
    [ToolkitModuleMethod(AccessOption.DeleteMessages)]
    public object DeleteMessage(TKDeleteMessageRequestModel model)
    {
        var result =  Options.MessageStorage.DeleteMessage(model.InboxId, model.MessageId);
        return new { Success = result };
    }

    /// <summary>
    /// Delete a single inbox by id.
    /// </summary>
    [ToolkitModuleMethod(AccessOption.DeleteMessages)]
    public object DeleteInbox(string id)
    {
        var result =  Options.MessageStorage.DeleteInbox(id);
        return new { Success = result };
    }

    /// <summary>
    /// Delete all stored messages.
    /// </summary>
    [ToolkitModuleMethod(AccessOption.DeleteMessages)]
    public object DeleteAllData()
    {
        Options.MessageStorage.DeleteAllData();
        return new { Success = true };
    }

    /// <summary>
    /// Get inbox metadata details.
    /// </summary>
    [ToolkitModuleMethod]
    public IEnumerable<TKMessagesInboxMetadata> GetInboxMetadata()
        => Options.InboxMetadata;

    /// <summary>
    /// Get latest messages from the given inbox.
    /// </summary>
    [ToolkitModuleMethod]
    public TKDataWithTotalCount<IEnumerable<ITKMessageItem>> GetLatestInboxMessages(ToolkitModuleContext context, TKGetMessagesRequestModel model)
    {
        var emptyResult = new TKDataWithTotalCount<IEnumerable<ITKMessageItem>>() { Data = Enumerable.Empty<ITKMessageItem>() };
        if (Options.MessageStorage == null)
        {
            return emptyResult;
        }

        var metadatas = GetInboxMetadata();
        if (!metadatas.Any(x => x.Id == model.InboxId))
        {
            return emptyResult;
        }

        var messageContainer = Options.MessageStorage.GetLatestMessages(model.InboxId, model.PageSize, model.PageIndex);
        if (messageContainer == null || !messageContainer.Data.Any())
        {
            return emptyResult;
        }

        context.AddAuditEvent(action: "Messages fetched", subject: model.InboxId)
            .AddDetail("Inbox id", model.InboxId)
            .AddDetail("Page index", model.PageIndex.ToString())
            .AddDetail("Page size", model.PageSize.ToString());

        return messageContainer;
    }

    /// <summary>
    /// Get a single messages from the given inbox.
    /// </summary>
    [ToolkitModuleMethod]
    public ITKMessageItem GetMessage(ToolkitModuleContext context, TKGetMessageRequestModel model)
    {
        if (Options.MessageStorage == null)
        {
            return null;
        }

        var metadatas = GetInboxMetadata();
        if (!metadatas.Any())
        {
            return null;
        }

        var message = Options.MessageStorage.GetMessage(model.InboxId, model.MessageId);

        if (message != null)
        {
            var subject = context.TryStripSensitiveData(message.Summary);
            context.AddAuditEvent(action: "Message fetched", subject: subject)
                .AddDetail("Inbox id", model.InboxId)
                .AddDetail("Message id", model.MessageId);
        }

        return message;
    }
    #endregion
}
