import DataFlowPropertyDisplayInfo from "./DataFlowPropertyDisplayInfo";

export default interface DataflowStreamMetadata {
    Id: string;
    Name: string;
    Description: string | null;
    GroupName: string;
    SupportsFilterByDate: boolean;
    DateTimePropertyNameForUI: string | null;
    PropertyDisplayInfo: Array<DataFlowPropertyDisplayInfo>;
}
