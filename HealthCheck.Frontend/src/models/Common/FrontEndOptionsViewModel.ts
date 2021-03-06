export default interface FrontEndOptionsViewModel {
    ApplicationTitle: string;
    ApplicationTitleLink: string;
    InludeQueryStringInApiCalls: boolean;
    InvokeModuleMethodEndpoint: string;
    EndpointBase: string;
    EditorConfig: EditorWorkerConfig;
    
    ShowIntegratedLogin: boolean;
    IntegratedLoginEndpoint: string;
    IntegratedLoginShow2FA: boolean;
    IntegratedLoginSend2FACodeEndpoint: string;
    IntegratedLoginSend2FACodeButtonText: string;
    IntegratedLoginCurrent2FACodeExpirationTime: string | null;
    IntegratedLogin2FACodeLifetime: number;
}

export interface EditorWorkerConfig {
    EditorWorkerUrl: string;
    JsonWorkerUrl: string;
}