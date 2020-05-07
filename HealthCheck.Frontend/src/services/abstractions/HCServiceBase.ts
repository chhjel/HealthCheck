import FrontEndOptionsViewModel from "../../models/Page/FrontEndOptionsViewModel";

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

export default class HCServiceBase
{
    public options: FrontEndOptionsViewModel;

    constructor(options: FrontEndOptionsViewModel)
    {
        this.options = options;
    }

    public fetchExt<T>(
        url: string,
        method: string,
        payload: any | null = null,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<T> | null = null
    ): void
    {
        if (statusObject != null)
        {
            statusObject.inProgress = true;
            statusObject.failed = false;
            statusObject.errorMessage = null;
        }

        let queryStringIfEnabled = this.options.InludeQueryStringInApiCalls ? window.location.search : '';
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
            if (response.redirected)
            {
                throw new Error('Was redirected. Probably not logged in.');
            }

            return response.json();
        })
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

