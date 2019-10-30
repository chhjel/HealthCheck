import { FilterQueryMode } from "./FilterQueryMode";

export default interface LogSearchFilter {
    SearchId: string;
    Skip: number;
    Take: number;
    FromDate: Date | null;
    ToDate: Date | null;
    Query: string;
    QueryMode: FilterQueryMode;
    ExcludedQuery: string;
    ExcludedQueryMode: FilterQueryMode;
    LogPathQuery: string;
    LogPathQueryMode: FilterQueryMode;
    ExcludedLogPathQuery: string;
    ExcludedLogPathQueryMode: FilterQueryMode;
    ColumnRegexPattern: string;
    ColumnDelimiter: string;
    OrderDescending: boolean;
    MaxStatisticsCount: number;
    MarginMilliseconds: number;
}