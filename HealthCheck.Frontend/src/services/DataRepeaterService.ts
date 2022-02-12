import { HCDataRepeaterAnalyzeItemRequest } from './../generated/Models/Core/HCDataRepeaterAnalyzeItemRequest';
import { HCDataRepeaterStreamItemDetailsViewModel } from './../generated/Models/Core/HCDataRepeaterStreamItemDetailsViewModel';
import { HCDataItemChangeBase } from "generated/Models/Core/HCDataItemChangeBase";
import { HCDataRepeaterPerformItemActionRequest } from "generated/Models/Core/HCDataRepeaterPerformItemActionRequest";
import { HCDataRepeaterRetryItemRequest } from "generated/Models/Core/HCDataRepeaterRetryItemRequest";
import { HCDataRepeaterRetryResult } from "generated/Models/Core/HCDataRepeaterRetryResult";
import { HCDataRepeaterStreamItemActionResult } from "generated/Models/Core/HCDataRepeaterStreamItemActionResult";
import { HCDataRepeaterStreamItemsPagedViewModel } from "generated/Models/Core/HCDataRepeaterStreamItemsPagedViewModel";
import { HCDataRepeaterStreamItemViewModel } from "generated/Models/Core/HCDataRepeaterStreamItemViewModel";
import { HCGetDataRepeaterItemDetailsRequest } from "generated/Models/Core/HCGetDataRepeaterItemDetailsRequest";
import { HCGetDataRepeaterStreamDefinitionsViewModel } from "generated/Models/Core/HCGetDataRepeaterStreamDefinitionsViewModel";
import { HCGetDataRepeaterStreamItemsFilteredRequest } from "generated/Models/Core/HCGetDataRepeaterStreamItemsFilteredRequest";
import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";
import { HCDataRepeaterItemAnalysisResult } from 'generated/Models/Core/HCDataRepeaterItemAnalysisResult';
import { HCDataRepeaterPerformBatchActionRequest } from 'generated/Models/Core/HCDataRepeaterPerformBatchActionRequest';
import { HCDataRepeaterStreamBatchActionResult } from 'generated/Models/Core/HCDataRepeaterStreamBatchActionResult';

export interface HCDataRepeaterResultWithItem<TData>
{
    Item: HCDataRepeaterStreamItemViewModel;
    Data: TData;
}

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
    
    public GetStreamItemsPaged(
        payload: HCGetDataRepeaterStreamItemsFilteredRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCDataRepeaterStreamItemsPagedViewModel | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetStreamItemsPaged", payload, statusObject, callbacks);
    }
    
    public GetItemDetails(
        payload: HCGetDataRepeaterItemDetailsRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCDataRepeaterStreamItemDetailsViewModel | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetItemDetails", payload, statusObject, callbacks);
    }
    
    public AnalyseItem(
        payload: HCDataRepeaterAnalyzeItemRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCDataRepeaterResultWithItem<HCDataRepeaterItemAnalysisResult> | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "AnalyseItem", payload, statusObject, callbacks);
    }
    
    public RetryItem(
        payload: HCDataRepeaterRetryItemRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCDataRepeaterResultWithItem<HCDataRepeaterRetryResult> | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "RetryItem", payload, statusObject, callbacks);
    }
    
    public PerformItemAction(
        payload: HCDataRepeaterPerformItemActionRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCDataRepeaterResultWithItem<HCDataRepeaterStreamItemActionResult> | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "PerformItemAction", payload, statusObject, callbacks);
    }
    
    public PerformBatchAction(
        payload: HCDataRepeaterPerformBatchActionRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCDataRepeaterStreamBatchActionResult> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "PerformBatchAction", payload, statusObject, callbacks);
    }
}
