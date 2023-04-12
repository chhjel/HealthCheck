import { TKDataRepeaterAnalyzeItemRequest } from './../generated/Models/Core/TKDataRepeaterAnalyzeItemRequest';
import { TKDataRepeaterStreamItemDetailsViewModel } from './../generated/Models/Core/TKDataRepeaterStreamItemDetailsViewModel';
import { TKDataItemChangeBase } from "@generated/Models/Core/TKDataItemChangeBase";
import { TKDataRepeaterPerformItemActionRequest } from "@generated/Models/Core/TKDataRepeaterPerformItemActionRequest";
import { TKDataRepeaterRetryItemRequest } from "@generated/Models/Core/TKDataRepeaterRetryItemRequest";
import { TKDataRepeaterRetryResult } from "@generated/Models/Core/TKDataRepeaterRetryResult";
import { TKDataRepeaterStreamItemActionResult } from "@generated/Models/Core/TKDataRepeaterStreamItemActionResult";
import { TKDataRepeaterStreamItemsPagedViewModel } from "@generated/Models/Core/TKDataRepeaterStreamItemsPagedViewModel";
import { TKDataRepeaterStreamItemViewModel } from "@generated/Models/Core/TKDataRepeaterStreamItemViewModel";
import { TKGetDataRepeaterItemDetailsRequest } from "@generated/Models/Core/TKGetDataRepeaterItemDetailsRequest";
import { TKGetDataRepeaterStreamDefinitionsViewModel } from "@generated/Models/Core/TKGetDataRepeaterStreamDefinitionsViewModel";
import { TKGetDataRepeaterStreamItemsFilteredRequest } from "@generated/Models/Core/TKGetDataRepeaterStreamItemsFilteredRequest";
import TKServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/TKServiceBase";
import { TKDataRepeaterItemAnalysisResult } from '@generated/Models/Core/TKDataRepeaterItemAnalysisResult';
import { TKDataRepeaterPerformBatchActionRequest } from '@generated/Models/Core/TKDataRepeaterPerformBatchActionRequest';
import { TKDataRepeaterStreamBatchActionResult } from '@generated/Models/Core/TKDataRepeaterStreamBatchActionResult';

export interface TKDataRepeaterResultWithItem<TData>
{
    Item: TKDataRepeaterStreamItemViewModel;
    Data: TData;
}

export default class DataRepeaterService extends TKServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    public GetStreamDefinitions(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKGetDataRepeaterStreamDefinitionsViewModel | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetStreamDefinitions", null, statusObject, callbacks);
    }
    
    public GetStreamItemsPaged(
        payload: TKGetDataRepeaterStreamItemsFilteredRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKDataRepeaterStreamItemsPagedViewModel | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetStreamItemsPaged", payload, statusObject, callbacks);
    }
    
    public GetItemDetails(
        payload: TKGetDataRepeaterItemDetailsRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKDataRepeaterStreamItemDetailsViewModel | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetItemDetails", payload, statusObject, callbacks);
    }
    
    public AnalyseItem(
        payload: TKDataRepeaterAnalyzeItemRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKDataRepeaterResultWithItem<TKDataRepeaterItemAnalysisResult> | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "AnalyseItem", payload, statusObject, callbacks);
    }
    
    public RetryItem(
        payload: TKDataRepeaterRetryItemRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKDataRepeaterResultWithItem<TKDataRepeaterRetryResult> | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "RetryItem", payload, statusObject, callbacks);
    }
    
    public PerformItemAction(
        payload: TKDataRepeaterPerformItemActionRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKDataRepeaterResultWithItem<TKDataRepeaterStreamItemActionResult> | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "PerformItemAction", payload, statusObject, callbacks);
    }
    
    public PerformBatchAction(
        payload: TKDataRepeaterPerformBatchActionRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKDataRepeaterStreamBatchActionResult> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "PerformBatchAction", payload, statusObject, callbacks);
    }
}
