import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";

export interface HCIntegratedLoginRequest
{
    username: string;
    password: string;
    twoFactorCode: string;
}
export interface HCIntegratedLoginResult
{
    success: boolean;
    errorMessage: string;
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
}
