import { LogEntrySeverity } from "./LogEntrySeverity";

export default interface LogEntrySearchResultItem {
    Timestamp: Date;
    FilePath: string;
    LineNumber: number;
    Raw: string;
    ColumnValues: Array<string>;
    IsMargin: boolean;
    Severity: LogEntrySeverity;
}
