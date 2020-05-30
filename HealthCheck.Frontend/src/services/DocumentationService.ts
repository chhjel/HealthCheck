import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";
import DiagramsDataViewModel from "../models/modules/Documentation/DiagramsDataViewModel";

export default class DocumentationService extends HCServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    public GetDiagramsData(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<DiagramsDataViewModel> | null = null
    ) : void
    {
        this.invokeModuleMethod(this.moduleId, 'GetDiagrams', null, statusObject, callbacks);
    }
}
