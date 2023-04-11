import { TKDataflowUnifiedSearchResult } from './../generated/Models/Core/TKDataflowUnifiedSearchResult';
import { TKDataFlowUnifiedSearchRequest } from './../generated/Models/Core/TKDataFlowUnifiedSearchRequest';
import TKServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/TKServiceBase";
import DataflowStreamMetadata from "../models/modules/Dataflow/DataflowStreamMetadata";
import DataflowEntry from "../models/modules/Dataflow/DataflowEntry";
import GetDataflowEntriesRequestModel from "../models/modules/Dataflow/GetDataflowEntriesRequestModel";
import DataflowUnifiedSearchMetadata from "@models/modules/Dataflow/DataflowUnifiedSearchMetadata";

export default class DataflowService extends TKServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    public GetStreamMetadata(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<DataflowStreamMetadata>> | null = null
    ) : void
    {
        this.invokeModuleMethod(this.moduleId, "GetDataflowStreamsMetadata", null, statusObject, callbacks);
    }

    public GetStreamEntries(
        filter: GetDataflowEntriesRequestModel,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<DataflowEntry>> | null = null
    ) : void
    {
        this.invokeModuleMethod(this.moduleId, "GetDataflowStreamEntries", filter, statusObject, callbacks);
    }
    
    public GetUnifiedSearchMetadata(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<DataflowUnifiedSearchMetadata>> | null = null
    ) : void
    {
        this.invokeModuleMethod(this.moduleId, "GetDataflowUnifiedSearchMetadata", null, statusObject, callbacks);
    }

    public UnifiedSearch(
        model: TKDataFlowUnifiedSearchRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKDataflowUnifiedSearchResult> | null = null
    ) : void
    {
        this.invokeModuleMethod(this.moduleId, "UnifiedSearch", model, statusObject, callbacks);
    }
}
