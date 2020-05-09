import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";
import LogSearchFilter from "../models/LogViewer/LogSearchFilter";
import LogSearchResult from "../models/LogViewer/LogSearchResult";

export default class LogService extends HCServiceBase
{
    public Search(filter: Partial<LogSearchFilter>,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<LogSearchResult> | null = null
    ): void {
        let url = this.options.GetLogSearchResultsEndpoint;
        this.fetchExt<any>(url, 'POST', filter, statusObject, callbacks);
    }
    
    public CancelSearch(searchId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<boolean> | null = null
    ): void {
        let url = this.options.CancelLogSearchEndpoint;
        this.fetchExt<boolean>(url, 'POST', { searchId: searchId }, statusObject, callbacks);
    }
    
    public CancelAllSearches(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<number> | null = null
    ): void {
        let url = this.options.CancelAllLogSearchesEndpoint;
        this.fetchExt<number>(url, 'POST', null, statusObject, callbacks);
    }
}
