import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";
import DiagramsDataViewModel from "../models/Documentation/DiagramsDataViewModel";

export default class DocumentationService extends HCServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    // this.invokeModuleMethod(this.moduleId, 'GetSiteEvents', {}, statusObject, callbacks);
    
    public GetDiagramsData(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<DiagramsDataViewModel> | null = null
    ) : void
    {
        // this.fetchExt<DiagramsDataViewModel>(url, 'GET', null, statusObject, callbacks);
    }
}
