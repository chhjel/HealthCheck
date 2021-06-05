import { HCVerifyWebAuthnAssertionModel } from 'generated/Models/WebUI/HCVerifyWebAuthnAssertionModel';
import { HCGenericResult } from './../generated/Models/Core/HCGenericResult';
import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";

export default class IntegratedProfileService extends HCServiceBase
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
        callbacks: ServiceFetchCallbacks<HCGenericResult> | null = null
    ): void {
        const url = `${this._endpointBase}/ProfileElevateTotp`;
        const payload = {
            Code: code
        };
        this.fetchExt<HCGenericResult>(url, 'POST', payload, statusObject, callbacks, true);
    }

    public RegisterTotp(
        secret: string, code: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCGenericResult> | null = null
    ): void {
        const url = `${this._endpointBase}/ProfileRegisterTotp`;
        const payload = {
            Secret: secret,
            Code: code
        };
        this.fetchExt<HCGenericResult>(url, 'POST', payload, statusObject, callbacks, true);
    }
    
    public RemoveTotp(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCGenericResult> | null = null
    ): void {
        const url = `${this._endpointBase}/ProfileRemoveTotp`;
        this.fetchExt<HCGenericResult>(url, 'POST', null, statusObject, callbacks, true);
    }
    
    public ElevateWebAuthn(
        data: HCVerifyWebAuthnAssertionModel,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCGenericResult> | null = null
    ): void {
        const url = `${this._endpointBase}/ProfileElevateWebAuthn`;
        const payload = {
            Data: data
        };
        this.fetchExt<HCGenericResult>(url, 'POST', payload, statusObject, callbacks, true);
    }
    
    public RegisterWebAuthn(
        payload: any,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCGenericResult> | null = null
    ): void {
        const url = `${this._endpointBase}/ProfileRegisterWebAuthn`;
        this.fetchExt<HCGenericResult>(url, 'POST', payload, statusObject, callbacks, true);
    }
    
    public RemoveWebAuthn(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCGenericResult> | null = null
    ): void {
        const url = `${this._endpointBase}/ProfileRemoveWebAuthn`;
        this.fetchExt<HCGenericResult>(url, 'POST', null, statusObject, callbacks, true);
    }

    public CreateWebAuthnRegistrationOptions(
        username: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<any> | null = null
    ): void {
        const url = `${this._endpointBase}/ProfileCreateWebAuthnRegistrationOptions`;
        this.fetchExt<any>(url, 'POST', { Username: username }, statusObject, callbacks, true);
    }
    
    // public RegisterWebAuthn(
    //     // url: string,
    //     payload: any,
    //     statusObject: FetchStatus | null = null,
    //     callbacks: ServiceFetchCallbacks<any> | null = null
    // ): void {
    //     const url = '/hclogin/RegisterWebAuthn';
    //     this.fetchExt<any>(url, 'POST', payload, statusObject, callbacks, true);
    // }
}
