import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";
import DataflowStreamMetadata from "../models/Dataflow/DataflowStreamMetadata";
import DataflowEntry from "../models/Dataflow/DataflowEntry";
import GetDataflowEntriesRequestModel from "../models/Dataflow/GetDataflowEntriesRequestModel";

export default class DataflowService extends HCServiceBase
{
    public GetStreamMetadata(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<DataflowStreamMetadata>> | null = null
    ) : void
    {
        let url = this.options.GetDataflowStreamsMetadataEndpoint;
        this.fetchExt<Array<DataflowStreamMetadata>>(url, 'GET', null, statusObject, callbacks);
    }

    public GetStreamEntries(
        filter: GetDataflowEntriesRequestModel,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<DataflowEntry>> | null = null
    ) : void
    {
        let url = this.options.GetDataflowStreamEntriesEndpoint;
        this.fetchExt<Array<DataflowEntry>>(url, 'POST', filter, statusObject, callbacks);
    }
}
