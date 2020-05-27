import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";
import SiteEventViewModel from "../models/SiteEvents/SiteEventViewModel";

export default class OverviewService extends HCServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    public GetSiteEvents(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<SiteEventViewModel>> | null = null
    ) : void
    {
        this.invokeModuleMethod(this.moduleId, 'GetSiteEvents', {}, statusObject, callbacks);
    }
}
