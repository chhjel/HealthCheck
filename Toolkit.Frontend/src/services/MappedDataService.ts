import { TKMappedExampleValueViewModel } from './../generated/Models/Core/TKMappedExampleValueViewModel';
import { TKMappedDataDefinitionsViewModel } from './../generated/Models/Core/TKMappedDataDefinitionsViewModel';
import TKServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/TKServiceBase";

export default class MappedDataService extends TKServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    public GetDefinitions(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKMappedDataDefinitionsViewModel | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetDefinitions", null, statusObject, callbacks);
    }
    
    public GetExampleValues(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<TKMappedExampleValueViewModel> | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetExampleValues", null, statusObject, callbacks);
    }
}