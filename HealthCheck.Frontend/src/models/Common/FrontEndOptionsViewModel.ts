export default interface FrontEndOptionsViewModel {
    ApplicationTitle: string;
    ApplicationTitleLink: string;
    InludeQueryStringInApiCalls: boolean;
    InvokeModuleMethodEndpoint: string;
    EndpointBase: string;
    EditorConfig: EditorWorkerConfig;
    ShowIntegratedLogin: boolean;
    IntegratedLoginEndpoint: string;
}

export interface EditorWorkerConfig {
    EditorWorkerUrl: string;
    JsonWorkerUrl: string;
}