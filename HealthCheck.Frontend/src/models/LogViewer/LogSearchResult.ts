import LogEntrySearchResultItem from "./LogEntrySearchResultItem";

export default interface LogSearchResult {
    TotalCount: number;
    Count: number;
    ColumnNames: Array<string>;
    Items: Array<LogEntrySearchResultItem>;
    DurationInMilliseconds: number;
    WasCancelled: boolean;
}