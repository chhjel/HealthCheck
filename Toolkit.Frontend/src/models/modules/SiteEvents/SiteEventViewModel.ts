import { SiteEventSeverity } from "./SiteEventSeverity";
import HyperLinkViewModel from "../../Common/HyperLinkViewModel";

export default interface SiteEventViewModel {
    Id: string;
    Severity: SiteEventSeverity;
    SeverityCode: number;
    Timestamp: Date;
    EndTime: Date;
    EventTypeId: string;
    Title: string;
    Description: string;
    DeveloperDetails: string;
    Duration: number;
    RelatedLinks: Array<HyperLinkViewModel>;
    Resolved: boolean;
    ResolvedMessage: string | null;
    ResolvedAt: Date | null;
}