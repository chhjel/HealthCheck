import { LogEntrySeverity } from "@models/modules/LogViewer/LogEntrySeverity";

export interface ChartEntry {
    date: Date;
    label: string;
    severity: LogEntrySeverity;
}

