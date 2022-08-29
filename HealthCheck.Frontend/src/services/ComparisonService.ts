import { HCComparisonDifferDefinition } from "@generated/Models/Core/HCComparisonDifferDefinition";
import { HCComparisonInstanceSelection } from "@generated/Models/Core/HCComparisonInstanceSelection";
import { HCComparisonMultiDifferOutput } from "@generated/Models/Core/HCComparisonMultiDifferOutput";
import { HCComparisonTypeDefinition } from "@generated/Models/Core/HCComparisonTypeDefinition";
import { HCExecuteDiffRequestModel } from "@generated/Models/Core/HCExecuteDiffRequestModel";
import { HCGetFilteredOptionsRequestModel } from "@generated/Models/Core/HCGetFilteredOptionsRequestModel";
import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";

export type DifferDefinitionsByHandlerId = { [key: string]: Array<HCComparisonDifferDefinition> };
export default class ComparisonService extends HCServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }

    public GetComparisonTypeDefinitions(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<HCComparisonTypeDefinition> | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetComparisonTypeDefinitions", null, statusObject, callbacks);
    }

    public GetDifferDefinitionsByHandlerId(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<DifferDefinitionsByHandlerId> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetDifferDefinitionsByHandlerId", null, statusObject, callbacks);
    }
    
    public GetFilteredOptions(
        handlerId: string, input: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<HCComparisonInstanceSelection>> | null = null
    ): void {
        const payload: HCGetFilteredOptionsRequestModel = {
            HandlerId: handlerId,
            Input: input
        };
        this.invokeModuleMethod(this.moduleId, "GetFilteredOptions", payload, statusObject, callbacks);
    }
    
    public ExecuteDiff(
        handlerId: string, differIds: string[], leftId: string, rightId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCComparisonMultiDifferOutput> | null = null
    ): void {
        const payload: HCExecuteDiffRequestModel = {
            HandlerId: handlerId,
            DifferIds: differIds,
            LeftId: leftId,
            RightId: rightId
        };
        this.invokeModuleMethod(this.moduleId, "ExecuteDiff", payload, statusObject, callbacks);
    }
}
