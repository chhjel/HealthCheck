import { HCDataRepeaterStreamItemDetailsViewModel } from './../generated/Models/Core/HCDataRepeaterStreamItemDetailsViewModel';
import { HCDataItemChangeBase } from "generated/Models/Core/HCDataItemChangeBase";
import { HCDataRepeaterPerformItemActionRequest } from "generated/Models/Core/HCDataRepeaterPerformItemActionRequest";
import { HCDataRepeaterRetryItemRequest } from "generated/Models/Core/HCDataRepeaterRetryItemRequest";
import { HCDataRepeaterRetryResult } from "generated/Models/Core/HCDataRepeaterRetryResult";
import { HCDataRepeaterSimpleLogEntry } from "generated/Models/Core/HCDataRepeaterSimpleLogEntry";
import { HCDataRepeaterStreamItemActionResult } from "generated/Models/Core/HCDataRepeaterStreamItemActionResult";
import { HCDataRepeaterStreamItemDetails } from "generated/Models/Core/HCDataRepeaterStreamItemDetails";
import { HCDataRepeaterStreamItemsPagedViewModel } from "generated/Models/Core/HCDataRepeaterStreamItemsPagedViewModel";
import { HCDataRepeaterStreamItemViewModel } from "generated/Models/Core/HCDataRepeaterStreamItemViewModel";
import { HCGetDataRepeaterItemDetailsRequest } from "generated/Models/Core/HCGetDataRepeaterItemDetailsRequest";
import { HCGetDataRepeaterStreamDefinitionsViewModel } from "generated/Models/Core/HCGetDataRepeaterStreamDefinitionsViewModel";
import { HCGetDataRepeaterStreamItemsFilteredRequest } from "generated/Models/Core/HCGetDataRepeaterStreamItemsFilteredRequest";
import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";

export interface HCDataRepeaterResultWithLogMessage<TData>
{
    Data: TData;
    LogMessage: HCDataRepeaterSimpleLogEntry;
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
    
    public RetryItem(
        payload: HCDataRepeaterRetryItemRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCDataRepeaterResultWithLogMessage<HCDataRepeaterRetryResult> | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "RetryItem", payload, statusObject, callbacks);
    }
    
    public PerformItemAction(
        payload: HCDataRepeaterPerformItemActionRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCDataRepeaterResultWithLogMessage<HCDataRepeaterStreamItemActionResult> | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "PerformItemAction", payload, statusObject, callbacks);
    }

    public ApplyChanges(item: HCDataRepeaterStreamItemViewModel| null, changes: HCDataItemChangeBase | null)
    {
        if (changes == null || item == null) return;

        if (changes?.AllowRetry != null)
        {
            item.AllowRetry = changes.AllowRetry;
        }

        if (changes.RemoveAllTags && item.Tags && item.Tags.length > 0)
        {
            item.Tags = [];
        }

        if (changes?.TagsThatShouldNotExist && changes.TagsThatShouldNotExist.length > 0)
        {
            changes.TagsThatShouldNotExist.forEach(t => {
                item.Tags = item.Tags.filter(x => x != t);
            });
        }

        if (changes?.TagsThatShouldExist && changes?.TagsThatShouldExist.length > 0)
        {
            changes.TagsThatShouldExist.forEach(t => {
                if (!item.Tags.includes(t))
                {
                    item.Tags.push(t);
                }
            });
        }
    }
}
