import { TKComparisonDifferDefinition } from "@generated/Models/Core/TKComparisonDifferDefinition";
import { TKComparisonInstanceSelection } from "@generated/Models/Core/TKComparisonInstanceSelection";
import { TKComparisonMultiDifferOutput } from "@generated/Models/Core/TKComparisonMultiDifferOutput";
import { TKComparisonTypeDefinition } from "@generated/Models/Core/TKComparisonTypeDefinition";
import { TKExecuteDiffRequestModel } from "@generated/Models/Core/TKExecuteDiffRequestModel";
import { TKGetFilteredOptionsRequestModel } from "@generated/Models/Core/TKGetFilteredOptionsRequestModel";
import TKServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/TKServiceBase";

export type DifferDefinitionsByHandlerId = { [key: string]: Array<TKComparisonDifferDefinition> };
export default class ComparisonService extends TKServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }

    public GetComparisonTypeDefinitions(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<TKComparisonTypeDefinition> | null> | null = null
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
        callbacks: ServiceFetchCallbacks<Array<TKComparisonInstanceSelection>> | null = null
    ): void {
        const payload: TKGetFilteredOptionsRequestModel = {
            HandlerId: handlerId,
            Input: input
        };
        this.invokeModuleMethod(this.moduleId, "GetFilteredOptions", payload, statusObject, callbacks);
    }
    
    public ExecuteDiff(
        handlerId: string, differIds: string[], leftId: string, rightId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKComparisonMultiDifferOutput> | null = null
    ): void {
        const payload: TKExecuteDiffRequestModel = {
            HandlerId: handlerId,
            DifferIds: differIds,
            LeftId: leftId,
            RightId: rightId
        };
        this.invokeModuleMethod(this.moduleId, "ExecuteDiff", payload, statusObject, callbacks);
    }
}
