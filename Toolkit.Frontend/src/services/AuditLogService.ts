import TKServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/TKServiceBase";
import AuditEventFilterInputData from "../models/modules/AuditLog/AuditEventFilterInputData";
import AuditEventViewModel from "../models/modules/AuditLog/AuditEventViewModel";

export default class AuditLogService extends TKServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    public Search(filter: AuditEventFilterInputData,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<AuditEventViewModel>> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, 'GetFilteredAudits', filter, statusObject, callbacks);
    }

    public GetBlobContent(blobId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<string | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, 'GetAuditBlobContents', blobId, statusObject, callbacks);
    }
}
