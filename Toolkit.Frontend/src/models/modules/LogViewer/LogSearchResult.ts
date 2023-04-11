import LogEntrySearchResultItem from "./LogEntrySearchResultItem";
import LogSearchStatisticsResult from "./LogSearchStatisticsResult";
import ParsedQuery from "./ParsedQuery";
import KeyValuePair from "../../Common/KeyValuePair";

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
    GroupedEntries: Array<KeyValuePair<string, Array<LogEntrySearchResultItem>>>;
    ColumnNames: Array<string>;
    
    HighestDate: Date | null;
    LowestDate: Date | null;
    Statistics: Array<LogSearchStatisticsResult>;
    StatisticsIsComplete: boolean;

    ParsedQuery: ParsedQuery;
    ParsedExcludedQuery: ParsedQuery;
    ParsedLogPathQuery: ParsedQuery;
    ParsedExcludedLogPathQuery: ParsedQuery;
}