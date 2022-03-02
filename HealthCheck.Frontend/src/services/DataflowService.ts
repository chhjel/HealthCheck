import { HCDataflowUnifiedSearchResult } from './../generated/Models/Core/HCDataflowUnifiedSearchResult';
import { HCDataFlowUnifiedSearchRequest } from './../generated/Models/Core/HCDataFlowUnifiedSearchRequest';
import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";
import DataflowStreamMetadata from "../models/modules/Dataflow/DataflowStreamMetadata";
import DataflowEntry from "../models/modules/Dataflow/DataflowEntry";
import GetDataflowEntriesRequestModel from "../models/modules/Dataflow/GetDataflowEntriesRequestModel";
import DataflowUnifiedSearchMetadata from "models/modules/Dataflow/DataflowUnifiedSearchMetadata";

export default class DataflowService extends HCServiceBase
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
        model: HCDataFlowUnifiedSearchRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCDataflowUnifiedSearchResult> | null = null
    ) : void
    {
        this.invokeModuleMethod(this.moduleId, "UnifiedSearch", model, statusObject, callbacks);
    }
}
