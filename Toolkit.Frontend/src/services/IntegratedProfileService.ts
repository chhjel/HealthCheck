import { TKCreateWebAuthnRegistrationOptionsRequest } from './../generated/Models/WebUI/TKCreateWebAuthnRegistrationOptionsRequest';
import { TKVerifyWebAuthnAssertionModel } from '@generated/Models/WebUI/TKVerifyWebAuthnAssertionModel';
import { TKGenericResult } from '@generated/Models/Core/TKGenericResult';
import TKServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/TKServiceBase";

export default class IntegratedProfileService extends TKServiceBase
{
    private _endpointBase: string;

    constructor(endpointBase: string, inludeQueryString: boolean)
    {
        super('', inludeQueryString);
        this._endpointBase = endpointBase;
    }
    
    public ElevateTotp(
        code: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKGenericResult> | null = null
    ): void {
        const url = `${this._endpointBase}/ProfileElevateTotp`;
        const payload = {
            Code: code
        };
        this.fetchExt<TKGenericResult>(url, 'POST', payload, statusObject, callbacks, true);
    }

    public RegisterTotp(
        secret: string, code: string,
        password: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKGenericResult> | null = null
    ): void {
        const url = `${this._endpointBase}/ProfileRegisterTotp`;
        const payload = {
            Secret: secret,
            Code: code,
            Password: password
        };
        this.fetchExt<TKGenericResult>(url, 'POST', payload, statusObject, callbacks, true);
    }
    
    public RemoveTotp(
        password: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKGenericResult> | null = null
    ): void {
        const payload = {
            Password: password
        };
        const url = `${this._endpointBase}/ProfileRemoveTotp`;
        this.fetchExt<TKGenericResult>(url, 'POST', payload, statusObject, callbacks, true);
    }
    
    public CreateWebAuthnAssertionOptions(
        username: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<any> | null = null
    ): void {
        const url = `${this._endpointBase}/ProfileCreateWebAuthnAssertionOptions`;
        this.fetchExt<any>(url, 'POST', { Username: username }, statusObject, callbacks, true);
    }
    
    public ElevateWebAuthn(
        data: TKVerifyWebAuthnAssertionModel,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKGenericResult> | null = null
    ): void {
        const url = `${this._endpointBase}/ProfileElevateWebAuthn`;
        const payload = {
            Data: data
        };
        this.fetchExt<TKGenericResult>(url, 'POST', payload, statusObject, callbacks, true);
    }
    
    public RegisterWebAuthn(
        password: string,
        authData: any,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKGenericResult> | null = null
    ): void {
        const payload = {
            Password: password,
            RegistrationData: authData
        };
        const url = `${this._endpointBase}/ProfileRegisterWebAuthn`;
        this.fetchExt<TKGenericResult>(url, 'POST', payload, statusObject, callbacks, true);
    }
    
    public RemoveWebAuthn(
        password: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKGenericResult> | null = null
    ): void {
        const payload = {
            Password: password
        };
        const url = `${this._endpointBase}/ProfileRemoveWebAuthn`;
        this.fetchExt<TKGenericResult>(url, 'POST', payload, statusObject, callbacks, true);
    }

    public CreateWebAuthnRegistrationOptions(
        username: string,
        password: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<any> | null = null
    ): void {
        const payload: TKCreateWebAuthnRegistrationOptionsRequest = {
            UserName: username,
            Password: password
        };
        const url = `${this._endpointBase}/ProfileCreateWebAuthnRegistrationOptions`;
        this.fetchExt<any>(url, 'POST', payload, statusObject, callbacks, true);
    }
}
