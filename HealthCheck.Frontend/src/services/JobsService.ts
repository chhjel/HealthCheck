import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";

export default class JobsService extends HCServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    // public GetSomething(
    //     statusObject: FetchStatus | null = null,
    //     callbacks: ServiceFetchCallbacks<ReturnType | null> | null = null
    // ): void {
    //     this.invokeModuleMethod(this.moduleId, "EndpointName", null, statusObject, callbacks);
    // }
    
    // public DoSomething(
    //     handlerIds: string[], input: string,
    //     statusObject: FetchStatus | null = null,
    //     callbacks: ServiceFetchCallbacks<ReturnType> | null = null
    // ): void {
    //     const payload: SomeModel = {
    //         HandlerIds: handlerIds,
    //         Input: input
    //     };
    //     this.invokeModuleMethod(this.moduleId, "EndpointName", payload, statusObject, callbacks);
    // }
}