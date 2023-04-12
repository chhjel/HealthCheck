import { TKIntegratedLoginRequest } from "@generated/Models/WebUI/TKIntegratedLoginRequest";
import TKServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/TKServiceBase";

export interface TKIntegratedLoginResult
{
    Success: boolean;
    ErrorMessage: string;
    ShowErrorAsHtml: boolean;
}

export interface TKIntegratedLoginRequest2FACodeRequest
{
    Username: string;
}
export interface TKIntegratedLogin2FACodeRequestResult
{
    Success: boolean;
    SuccessMessage: string;
    ShowSuccessAsHtml: boolean;
    ErrorMessage: string;
    ShowErrorAsHtml: boolean;
    CodeExpiresInSeconds: number | null;
}

export default class IntegratedLoginService extends TKServiceBase
{
    constructor(inludeQueryString: boolean)
    {
        super('', inludeQueryString);
    }
    
    public Login(
        url: string,
        payload: TKIntegratedLoginRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKIntegratedLoginResult> | null = null
    ): void {
        this.fetchExt<TKIntegratedLoginResult>(url, 'POST', payload, statusObject, callbacks, true);
    }
    
    public RequestCode(
        url: string,
        payload: TKIntegratedLoginRequest2FACodeRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKIntegratedLogin2FACodeRequestResult> | null = null
    ): void {
        this.fetchExt<TKIntegratedLogin2FACodeRequestResult>(url, 'POST', payload, statusObject, callbacks, true);
    }

    // MFA: WebAuthn
    public CreateWebAuthnAssertionOptions(
        // url: string,
        username: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<any> | null = null
    ): void {
        const url = '/tklogin/CreateWebAuthnAssertionOptions';
        this.fetchExt<any>(url, 'POST', { Username: username }, statusObject, callbacks, true);
    }

    public VerifyAssertion(
        // url: string,
        payload: any,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<any> | null = null
    ): void {
        const url = '/tklogin/VerifyWebAuthnAssertion';
        this.fetchExt<any>(url, 'POST', payload, statusObject, callbacks, true);
    }
}
