import DataFlowPropertyDisplayInfo from "./DataFlowPropertyDisplayInfo";

export default interface DataflowStreamMetadata {
    Id: string;
    Name: string;
    Description: string | null;
    SupportsFilterByDate: boolean;
    PropertyDisplayInfo: Array<DataFlowPropertyDisplayInfo>;
}