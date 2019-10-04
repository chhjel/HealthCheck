import LogEntrySearchResultItem from "./LogEntrySearchResultItem";

export default interface LogSearchResult {
    Error: string | null;
    HasError: boolean;
    DurationInMilliseconds: number;
    WasCancelled: boolean;

    TotalCount: number;
    Count: number;
    PageCount: number;
    CurrentPage: number;

    Items: Array<LogEntrySearchResultItem>;
    ColumnNames: Array<string>;
    
    Dates: Array<Date>;
    HighestDate: Date | null;
    LowestDate: Date | null;
    AllDatesIncluded: boolean;
}