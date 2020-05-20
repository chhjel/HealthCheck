import FrontEndOptionsViewModel from "../../models/Common/FrontEndOptionsViewModel";

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
    public options: FrontEndOptionsViewModel;

    constructor(options: FrontEndOptionsViewModel)
    {
        this.options = options;
    }

    public invokeModuleMethod<T = unknown>(
        moduleId: string,
        methodName: string,
        payload: any | null = null,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<T> | null = null
    ): void
    {
        let url = this.options.InvokeModuleMethodEndpoint;
        
        let payloadJson = (payload == null) ? null : JSON.stringify(payload);
        let wrapperPayload = {
            moduleId: moduleId,
            methodName: methodName,
            jsonPayload: payloadJson
        };

        this.fetchExt<T>(url, 'POST', wrapperPayload, statusObject, callbacks, true);
    }

    public fetchExt<T = unknown>(
        url: string,
        method: string,
        payload: any | null = null,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<T> | null = null,
        json: boolean = true,
    ): void
    {
        if (statusObject != null)
        {
            statusObject.inProgress = true;
            statusObject.failed = false;
            statusObject.errorMessage = null;
        }

        let queryStringIfEnabled = '';
        if (this.options.InludeQueryStringInApiCalls)
        {
            queryStringIfEnabled = url.includes('?') ? `&${window.location.search.replace('?', '')}` : window.location.search;
        }
        url = `${url}${queryStringIfEnabled}`;

        let payloadJson = (payload == null) ? null : JSON.stringify(payload);
        fetch(url, {
            credentials: 'include',
            method: method,
            body: payloadJson,
            headers: new Headers({
                'Content-Type': 'application/json',
                Accept: 'application/json',
            })
        })
        .then(response => {
            if (response.redirected || response.status == 404 || response.status == 403)
            {
                return Promise.reject('Not logged in.');
            }

            if (json)
            {
                try {
                    return response.json();
                } catch {}
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

