import { SiteEventSeverity } from "./SiteEventSeverity";

export default interface SiteEventViewModel {
    Id: string;
    Severity: SiteEventSeverity;
    SeverityCode: number;
    Timestamp: Date;
    EventTypeId: string;
    Title: string;
    Description: string;
}