import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";
import SiteEventViewModel from "../models/SiteEvents/SiteEventViewModel";
import FrontEndOptionsViewModel from "../models/Common/FrontEndOptionsViewModel";

export default class OverviewService extends HCServiceBase
{
    public moduleId: string;

    constructor(options: FrontEndOptionsViewModel, moduleId: string)
    {
        super(options);
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
