export default interface FrontEndOptionsViewModel {
    ApplicationTitle: string;
    ApplicationTitleLink: string;
    InludeQueryStringInApiCalls: boolean;
    InvokeModuleMethodEndpoint: string;
    EndpointBase: string;
    EditorConfig: EditorWorkerConfig;
    ShowIntegratedLogin: boolean;
}

export interface EditorWorkerConfig {
    EditorWorkerUrl: string;
    JsonWorkerUrl: string;
}