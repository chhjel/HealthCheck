export default interface FrontEndOptionsViewModel {
    ApplicationTitle: string;
    ApplicationTitleLink: string;
    InludeQueryStringInApiCalls: boolean;
    GetTestsEndpoint: string;
    ExecuteTestEndpoint: string;
    CancelTestEndpoint: string;
    GetSiteEventsEndpoint: string;
    GetFilteredAuditLogEventsEndpoint: string;
    GetLogSearchResultsEndpoint: string;
    CancelLogSearchEndpoint: string;
    CancelAllLogSearchesEndpoint: string;
    GetRequestLogEndpoint: string;
    DefaultColumnRule: string;
    DefaultColumnModeIsRegex: boolean;
    ApplyCustomColumnRuleByDefault: boolean;
    CurrentEventBufferMinutes: number;
    CurrentlyRunningLogSearchCount: number;
    Pages: string[];
    MaxInsightsEntryCount: number;
    TestsTabName: string;
}