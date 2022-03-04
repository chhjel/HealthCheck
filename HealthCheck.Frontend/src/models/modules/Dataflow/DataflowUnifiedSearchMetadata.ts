export default interface DataflowUnifiedSearchMetadata {
    Id: string;
    Name: string;
    Description: string | null;
    GroupName: string;
    GroupByLabel: string | null;
    StreamNamesOverrides: { [key: string]: string } | null;
    GroupByStreamNamesOverrides: { [key: string]: string } | null;
}
