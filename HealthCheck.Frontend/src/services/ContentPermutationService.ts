import { HCGetPermutatedContentRequest } from '@generated/Models/Core/HCGetPermutatedContentRequest';
import { HCGetPermutationTypesViewModel } from '@generated/Models/Core/HCGetPermutationTypesViewModel';
import { HCGetPermutatedContentViewModel } from '@generated/Models/Core/HCGetPermutatedContentViewModel';
import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";

export default class ContentPermutationService extends HCServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    public GetPermutationTypes(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCGetPermutationTypesViewModel | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetPermutationTypes", null, statusObject, callbacks);
    }
    
    public GetPermutatedContent(
        payload: HCGetPermutatedContentRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCGetPermutatedContentViewModel | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetPermutatedContent", payload, statusObject, callbacks);
    }
}
