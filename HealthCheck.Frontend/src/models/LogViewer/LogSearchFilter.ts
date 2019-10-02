import { FilterQueryMode } from "./FilterQueryMode";

export default interface LogSearchFilter {
    SearchId: string;
    Skip: number;
    Take: number;
    FromFileDate: Date | null;
    ToFileDate: Date | null;
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
}