import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";
import LogSearchResult from "../models/LogViewer/LogSearchResult";
import AuditEventFilterInputData from "../models/AuditLog/AuditEventFilterInputData";
import AuditEventViewModel from "../models/AuditLog/AuditEventViewModel";

export default class AuditLogService extends HCServiceBase
{
    public Search(filter: AuditEventFilterInputData,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<AuditEventViewModel>> | null = null
    ): void {
        let url = this.options.GetFilteredAuditLogEventsEndpoint;
        this.fetchExt<Array<AuditEventViewModel>>(url, 'POST', filter, statusObject, callbacks);
    }
}
