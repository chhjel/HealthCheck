import { TKGoToRequestModel } from './../generated/Models/Core/TKGoToRequestModel';
import { TKGoToResolvedDataWithResolverId } from './../generated/Models/Core/TKGoToResolvedDataWithResolverId';
import { TKGoToResolverDefinition } from './../generated/Models/Core/TKGoToResolverDefinition';
import TKServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/TKServiceBase";

export default class GoToService extends TKServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }

    public GetResolversDefinitions(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<TKGoToResolverDefinition> | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetResolversDefinitions", null, statusObject, callbacks);
    }

    public Goto(
        handlerIds: string[], input: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<TKGoToResolvedDataWithResolverId>> | null = null
    ): void {
        const payload: TKGoToRequestModel = {
            HandlerIds: handlerIds,
            Input: input
        };
        this.invokeModuleMethod(this.moduleId, "Goto", payload, statusObject, callbacks);
    }
}
