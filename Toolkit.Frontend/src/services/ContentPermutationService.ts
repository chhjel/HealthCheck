import { TKGetPermutatedContentRequest } from '@generated/Models/Core/TKGetPermutatedContentRequest';
import { TKGetPermutationTypesViewModel } from '@generated/Models/Core/TKGetPermutationTypesViewModel';
import { TKGetPermutatedContentViewModel } from '@generated/Models/Core/TKGetPermutatedContentViewModel';
import TKServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/TKServiceBase";

export default class ContentPermutationService extends TKServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    public GetPermutationTypes(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKGetPermutationTypesViewModel | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetPermutationTypes", null, statusObject, callbacks);
    }
    
    public GetPermutatedContent(
        payload: TKGetPermutatedContentRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKGetPermutatedContentViewModel | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetPermutatedContent", payload, statusObject, callbacks);
    }
}
