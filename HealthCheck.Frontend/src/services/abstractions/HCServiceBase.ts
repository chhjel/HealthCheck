import FrontEndOptionsViewModel from "../../models/Common/FrontEndOptionsViewModel";
import KeyValuePair from "../../models/Common/KeyValuePair";

export class FetchStatus
{
    public inProgress: boolean = false;
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
import { FetchStatus } from "../../services/abstractions/HCServiceBase";

service: RequestLogService = new RequestLogService(this.options);
loadStatus: FetchStatus = new FetchStatus();
clearStatus: FetchStatus = new FetchStatus();

this.service.PerformOperation(this.internalConfig.Id, null, {
    // onSuccess: (data) => etc,
    // onError: (message) => etc,
    // onDone: () => etc
});
*/

export default class HCServiceBase
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
        json: boolean = true
    ): void
    {
        let payloadJson = (payload == null) ? null : JSON.stringify(payload);
        let wrapperPayload = {
            moduleId: moduleId,
            methodName: methodName,
            jsonPayload: payloadJson
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
            queryStringIfEnabled = url.includes('?') ? `&${window.location.search.replace('?', '')}` : window.location.search;
        }
        url = `${url}${queryStringIfEnabled}`;

        let payloadJson = (payload == null) ? null : JSON.stringify(payload);
        fetch(url, {
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
        })
        // .then(response => new Promise<Array<T>>(resolve => setTimeout(() => resolve(response), 3000)))
        .then((data: T) => {
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

