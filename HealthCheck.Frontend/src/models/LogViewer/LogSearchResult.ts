import LogEntrySearchResultItem from "./LogEntrySearchResultItem";
import LogSearchStatisticsResult from "./LogSearchStatisticsResult";

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
    
    HighestDate: Date | null;
    LowestDate: Date | null;
    Statistics: Array<LogSearchStatisticsResult>;
    StatisticsIsComplete: boolean;
}