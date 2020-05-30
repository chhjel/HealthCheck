import DataflowStreamFilter from "./DataflowStreamFilter";

export default interface GetDataflowEntriesRequestModel {
    StreamId: string;
    StreamFilter: DataflowStreamFilter;
}
