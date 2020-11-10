import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";
import { DataWithTotalCount, MessageItem, MessagesInboxMetadata } from "../models/modules/Messages/MessagesModels";
import { SimpleResult } from "../models/Common/GenericEndpointControlResult";

export default class MessagesService extends HCServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    public GetMessage(
        inboxId: string, messageId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<MessageItem | null> | null = null
    ): void {
        let payload = {
            inboxId: inboxId,
            messageId: messageId
        };
        this.invokeModuleMethod(this.moduleId, 'GetMessage', payload, statusObject, callbacks);
    }

    public GetLatestInboxMessages(
        inboxId: string, pageSize: number, pageIndex: number,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<DataWithTotalCount<Array<MessageItem>>> | null = null
    ): void {
        let payload = {
            inboxId: inboxId,
            pageSize: pageSize,
            pageIndex: pageIndex
        };
        this.invokeModuleMethod(this.moduleId, 'GetLatestInboxMessages', payload, statusObject, callbacks);
    }

    public GetInboxMetadata(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<MessagesInboxMetadata>> | null = null
    ) : void
    {
        this.invokeModuleMethod(this.moduleId, 'GetInboxMetadata', null, statusObject, callbacks);
    }

    public DeleteMessage(
        inboxId: string, messageId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<SimpleResult> | null = null
    ) : void
    {
        const payload = { inboxId: inboxId, messageId: messageId };
        this.invokeModuleMethod(this.moduleId, 'DeleteMessage', payload, statusObject, callbacks);
    }

    public DeleteInbox(
        inboxId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<SimpleResult> | null = null
    ) : void
    {
        const payload = inboxId;
        this.invokeModuleMethod(this.moduleId, 'DeleteInbox', payload, statusObject, callbacks);
    }

    public DeleteAllData(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<SimpleResult> | null = null
    ) : void
    {
        this.invokeModuleMethod(this.moduleId, 'DeleteAllData', null, statusObject, callbacks);
    }
}
