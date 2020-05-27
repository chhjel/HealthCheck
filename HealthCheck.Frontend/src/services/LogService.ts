import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";
import LogSearchFilter from "../models/LogViewer/LogSearchFilter";
import LogSearchResult from "../models/LogViewer/LogSearchResult";

export default class LogService extends HCServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    public Search(filter: Partial<LogSearchFilter>,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<LogSearchResult> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, 'SearchLogs', filter, statusObject, callbacks);
    }
    
    public CancelSearch(searchId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<boolean> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, 'CancelLogSearch', searchId, statusObject, callbacks);
    }
    
    public CancelAllSearches(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<number> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, 'CancelAllLogSearches', null, statusObject, callbacks);
    }
}
