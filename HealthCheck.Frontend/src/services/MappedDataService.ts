import { HCMappedDataDefinitionsViewModel } from './../generated/Models/Core/HCMappedDataDefinitionsViewModel';
import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";

export default class MappedDataService extends HCServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    public GetDefinitions(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCMappedDataDefinitionsViewModel | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetDefinitions", null, statusObject, callbacks);
    }
}