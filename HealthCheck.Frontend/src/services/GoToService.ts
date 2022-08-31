import { HCGoToRequestModel } from './../generated/Models/Core/HCGoToRequestModel';
import { HCGoToResolvedDataWithResolverId } from './../generated/Models/Core/HCGoToResolvedDataWithResolverId';
import { HCGoToResolverDefinition } from './../generated/Models/Core/HCGoToResolverDefinition';
import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";

export default class GoToService extends HCServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }

    public GetResolversDefinitions(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<HCGoToResolverDefinition> | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetResolversDefinitions", null, statusObject, callbacks);
    }

    public Goto(
        handlerIds: string[], input: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<HCGoToResolvedDataWithResolverId>> | null = null
    ): void {
        const payload: HCGoToRequestModel = {
            HandlerIds: handlerIds,
            Input: input
        };
        this.invokeModuleMethod(this.moduleId, "Goto", payload, statusObject, callbacks);
    }
}
