import { HCIntegratedLoginRequest } from "@generated/Models/WebUI/HCIntegratedLoginRequest";
import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";

export interface HCIntegratedLoginResult
{
    Success: boolean;
    ErrorMessage: string;
    ShowErrorAsHtml: boolean;
}

export interface HCIntegratedLoginRequest2FACodeRequest
{
    Username: string;
}
export interface HCIntegratedLogin2FACodeRequestResult
{
    Success: boolean;
    SuccessMessage: string;
    ShowSuccessAsHtml: boolean;
    ErrorMessage: string;
    ShowErrorAsHtml: boolean;
    CodeExpiresInSeconds: number | null;
}

export default class IntegratedLoginService extends HCServiceBase
{
    constructor(inludeQueryString: boolean)
    {
        super('', inludeQueryString);
    }
    
    public Login(
        url: string,
        payload: HCIntegratedLoginRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCIntegratedLoginResult> | null = null
    ): void {
        this.fetchExt<HCIntegratedLoginResult>(url, 'POST', payload, statusObject, callbacks, true);
    }
    
    public RequestCode(
        url: string,
        payload: HCIntegratedLoginRequest2FACodeRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCIntegratedLogin2FACodeRequestResult> | null = null
    ): void {
        this.fetchExt<HCIntegratedLogin2FACodeRequestResult>(url, 'POST', payload, statusObject, callbacks, true);
    }

    // MFA: WebAuthn
    public CreateWebAuthnAssertionOptions(
        // url: string,
        username: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<any> | null = null
    ): void {
        const url = '/hclogin/CreateWebAuthnAssertionOptions';
        this.fetchExt<any>(url, 'POST', { Username: username }, statusObject, callbacks, true);
    }

    public VerifyAssertion(
        // url: string,
        payload: any,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<any> | null = null
    ): void {
        const url = '/hclogin/VerifyWebAuthnAssertion';
        this.fetchExt<any>(url, 'POST', payload, statusObject, callbacks, true);
    }
}
