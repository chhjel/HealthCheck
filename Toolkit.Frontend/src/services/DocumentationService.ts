import TKServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/TKServiceBase";
import DiagramsDataViewModel from "../models/modules/Documentation/DiagramsDataViewModel";

export default class DocumentationService extends TKServiceBase
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
