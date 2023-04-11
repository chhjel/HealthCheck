import TKServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/TKServiceBase";
import SiteEventViewModel from "../models/modules/SiteEvents/SiteEventViewModel";

export default class OverviewService extends TKServiceBase
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
    
    public ClearSiteEvents(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<any> | null = null
    ) : void
    {
        this.invokeModuleMethod(this.moduleId, 'ClearSiteEvents', {}, statusObject, callbacks);
    }
    
    public DeleteSiteEvent(
        id: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<any> | null = null
    ) : void
    {
        const payload = {
            id: id
        };
        this.invokeModuleMethod(this.moduleId, 'DeleteSiteEvent', payload, statusObject, callbacks);
    }
}
