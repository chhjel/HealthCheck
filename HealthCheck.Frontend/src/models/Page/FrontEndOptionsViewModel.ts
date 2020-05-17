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
    ClearRequestLogEndpoint: string;
    DiagramsDataEndpoint: string;
    GetDataflowStreamsMetadataEndpoint: string;
    GetDataflowStreamEntriesEndpoint: string;
    DefaultColumnRule: string;
    HasAccessToClearRequestLog: boolean;
    DefaultColumnModeIsRegex: boolean;
    ApplyCustomColumnRuleByDefault: boolean;
    CurrentEventBufferMinutes: number;
    CurrentlyRunningLogSearchCount: number;
    Pages: string[];
    MaxInsightsEntryCount: number;
    TestsTabName: string;
    EnableDiagramSandbox: boolean;
    EnableDiagramDetails: boolean;
    GetSettingsEndpoint: string;
    SetSettingsEndpoint: string;
    GetEventNotificationConfigsEndpoint: string;
    SaveEventNotificationConfigEndpoint: string;
    DeleteEventNotificationConfigEndpoint: string;
    SetEventNotificationConfigEnabledEndpoint: string;
    DeleteEventDefinitionEndpoint: string;
    DeleteEventDefinitionsEndpoint: string;
    HasAccessToEditEventDefinitions: boolean;
}