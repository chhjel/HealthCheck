import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";
import SiteEventViewModel from "../models/SiteEvents/SiteEventViewModel";

export default class OverviewService extends HCServiceBase
{
    public GetSiteEvents(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<SiteEventViewModel>> | null = null
    ) : void
    {
        let url = this.options.GetSiteEventsEndpoint;
        this.fetchExt<Array<SiteEventViewModel>>(url, 'GET', null, statusObject, callbacks);
    }
}
