import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";
import DataflowStreamMetadata from "../models/Dataflow/DataflowStreamMetadata";
import DataflowEntry from "../models/Dataflow/DataflowEntry";
import GetDataflowEntriesRequestModel from "../models/Dataflow/GetDataflowEntriesRequestModel";

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
        // this.fetchExt<Array<DataflowStreamMetadata>>(url, 'GET', null, statusObject, callbacks);
    }

    public GetStreamEntries(
        filter: GetDataflowEntriesRequestModel,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<DataflowEntry>> | null = null
    ) : void
    {
        // this.fetchExt<Array<DataflowEntry>>(url, 'POST', filter, statusObject, callbacks);
    }
}
