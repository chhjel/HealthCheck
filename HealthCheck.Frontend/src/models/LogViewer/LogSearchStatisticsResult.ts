import { LogEntrySeverity } from "./LogEntrySeverity";

export default interface LogSearchStatisticsResult {
    Timestamp: Date;
    Severity: LogEntrySeverity;
}