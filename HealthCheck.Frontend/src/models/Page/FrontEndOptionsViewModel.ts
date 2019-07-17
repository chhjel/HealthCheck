export default interface FrontEndOptionsViewModel {
    ApplicationTitle: string;
    InludeQueryStringInApiCalls: boolean;
    GetTestsEndpoint: string;
    ExecuteTestEndpoint: string;
    GetSiteEventsEndpoint: string;
    GetFilteredAuditLogEventsEndpoint: string;
    CurrentEventBufferMinutes: number;
    Pages: string[];
}