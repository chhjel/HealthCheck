export default interface AuditEventFilterInputData {
    FromFilter?: Date | null;
    ToFilter?: Date | null;
    AreaFilter: string | null;
    SubjectFilter: string;
    ActionFilter: string;
    UserIdFilter: string;
    UserNameFilter: string;
}
