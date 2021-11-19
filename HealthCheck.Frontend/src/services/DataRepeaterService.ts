import { HCGetDataRepeaterStreamDefinitionsViewModel } from "generated/Models/Core/HCGetDataRepeaterStreamDefinitionsViewModel";
import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";

export default class DataRepeaterService extends HCServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    public GetStreamDefinitions(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCGetDataRepeaterStreamDefinitionsViewModel | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetStreamDefinitions", null, statusObject, callbacks);
    }
}
