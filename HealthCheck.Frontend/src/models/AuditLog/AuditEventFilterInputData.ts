import { AuditEventArea } from "./AuditEventArea";

export default interface AuditEventFilterInputData {
    FromFilter?: Date | null;
    ToFilter?: Date | null;
    AreaFilter?: AuditEventArea | null;
    SubjectFilter: string;
    ActionFilter: string;
    UserIdFilter: string;
    UserNameFilter: string;
}
