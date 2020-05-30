import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";
import AuditEventFilterInputData from "../models/modules/AuditLog/AuditEventFilterInputData";
import AuditEventViewModel from "../models/modules/AuditLog/AuditEventViewModel";

export default class AuditLogService extends HCServiceBase
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
}
