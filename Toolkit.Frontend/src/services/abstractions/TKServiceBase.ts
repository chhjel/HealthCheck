import Base64Util from '@util/Base64Util';
import UrlUtils from '@util/UrlUtils';

export class FetchStatus
{
    public inProgress: boolean = false;
    public failed: boolean = false;
    public errorMessage: string | null = null;
}

export class FetchStatusWithProgress
{
    public inProgress: boolean = false;
    public progress: number = 0;
    public failed: boolean = false;
    public errorMessage: string | null = null;
}

export interface ServiceFetchCallbacks<T>
{
    onSuccess?: ((data: T) => void ) | null;
    onError?: ((message: string) => void) | null;
    onDone?: (() => void) | null;
}

/*
import RequestLogService from "../../services/RequestLogService";
import { FetchStatus } from "../../services/abstractions/TKServiceBase";

service: RequestLogService = new RequestLogService(this.options);
loadStatus: FetchStatus = new FetchStatus();
clearStatus: FetchStatus = new FetchStatus();

this.service.PerformOperation(this.internalConfig.Id, null, {
    // onSuccess: (data) => etc,
    // onError: (message) => etc,
    // onDone: () => etc
});
*/

export default class TKServiceBase
{
    private endpoint: string;
    private inludeQueryString: boolean;

    constructor(endpoint: string, inludeQueryString: boolean)
    {
        this.endpoint = endpoint;
        this.inludeQueryString = inludeQueryString;
    }

    public invokeModuleMethod<T = unknown>(
        moduleId: string,
        methodName: string,
        payload: any | null = null,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<T> | null = null,
        json: boolean = true,
        isB64: boolean = false
    ): void
    {
        let payloadJson = (payload == null) ? null : JSON.stringify(payload);
        if (payloadJson != null && isB64) { payloadJson = Base64Util.base64Encode(payloadJson) }

        let wrapperPayload = {
            moduleId: moduleId,
            methodName: methodName,
            jsonPayload: payloadJson,
            isB64: isB64
        };

        if (DEVMODE)
        {
            console.log({
                moduleId: moduleId,
                methodName: methodName,
                payload: payload
            });
        }
        this.fetchExt<T>(this.endpoint, 'POST', wrapperPayload, statusObject, callbacks, json);
    }

    public fetchExt<T = unknown>(
        url: string,
        method: string,
        payload: any | null = null,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<T> | null = null,
        json: boolean = true,
        headers: Record<string, string> | null = null
    ): void
    {
        if (statusObject != null)
        {
            statusObject.inProgress = true;
            statusObject.failed = false;
            statusObject.errorMessage = null;
        }

        let queryStringIfEnabled = '';
        if (this.inludeQueryString)
        {
            queryStringIfEnabled = window.location.search
            
            // Strip special h-parameter as it may be blocked by web application firewalls since it may contain e.g. a linq query.
            queryStringIfEnabled = UrlUtils.RemoveRelativeQueryStringParameter(queryStringIfEnabled, 'h');
            queryStringIfEnabled = url.includes('?') ? `&${queryStringIfEnabled.replace('?', '')}` : queryStringIfEnabled;
        }
        url = `${url}${queryStringIfEnabled}`;

        let payloadJson = (payload == null) ? null : JSON.stringify(payload);
        let promise = fetch(url, {
            credentials: 'include',
            method: method,
            body: payloadJson,
            headers: headers || new Headers({
                'Content-Type': 'application/json',
                Accept: 'application/json',
            })
        })
        .then(response => {
            if (response.redirected || response.status == 404 || response.status == 403)
            {
                return Promise.reject('Not logged in or no access.');
            }

            if (json)
            {
                return response.json();
            }
            
            return response;
        });

        if (window?.location?.href?.includes('__tk_simulate_slow=true') == true) {
            promise = promise.then(response => new Promise<Array<T>>(resolve => setTimeout(() => resolve(response), 3000)));
        }
        if (window?.location?.href?.includes('__tk_simulate_slow_f=true') == true) {
            promise = promise.then(response => new Promise<Array<T>>(resolve => setTimeout(() => resolve(response), 60000)));
        }

        promise.then((data: T) => {
            if (statusObject != null)
            {
                statusObject.inProgress = false;
            }

            if (callbacks != null && callbacks.onSuccess != null)
            {
                callbacks.onSuccess(data);
            }

            if (callbacks != null && callbacks.onDone != null)
            {
                callbacks.onDone();
            }
        })
        .catch((e) => {
            console.error(e);
            
            if (statusObject != null)
            {
                statusObject.inProgress = false;
                statusObject.failed = true;
                statusObject.errorMessage = `Failed to load data with the following error. ${e}.`;
            }

            if (callbacks != null && callbacks.onError != null)
            {
                callbacks.onError(`Failed to load data with the following error. ${e}.`);
            }

            if (callbacks != null && callbacks.onDone != null)
            {
                callbacks.onDone();
            }
        });
    }
}

