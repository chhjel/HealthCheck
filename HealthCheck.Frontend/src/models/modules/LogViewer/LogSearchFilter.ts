export default interface LogSearchFilter {
    SearchId: string;
    Skip: number;
    Take: number;
    FromDate: Date | null;
    ToDate: Date | null;
    Query: string;
    QueryIsRegex: boolean;
    ExcludedQuery: string;
    ExcludedQueryIsRegex: boolean;
    LogPathQuery: string;
    LogPathQueryIsRegex: boolean;
    ExcludedLogPathQuery: string;
    ExcludedLogPathQueryIsRegex: boolean;
    ColumnRegexPattern: string;
    ColumnDelimiter: string;
    OrderDescending: boolean;
    MaxStatisticsCount: number;
    MarginMilliseconds: number;
}