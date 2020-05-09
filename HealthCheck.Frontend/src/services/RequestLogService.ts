import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";
import LoggedEndpointDefinitionViewModel from "../models/RequestLog/LoggedEndpointDefinitionViewModel";

export default class RequestLogService extends HCServiceBase
{
    public GetRequestLog(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<LoggedEndpointDefinitionViewModel>> | null = null
    ) : void
    {
        let url = this.options.GetRequestLogEndpoint;
        this.fetchExt<Array<LoggedEndpointDefinitionViewModel>>(url, 'GET', null, statusObject, callbacks);
    }

    public ClearRequestLog(includeDefinitions: boolean,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<any> | null = null
    ): void
    {
        let url = `${this.options.ClearRequestLogEndpoint}?includeDefinitions=${includeDefinitions}`;
        this.fetchExt<any>(url, 'DELETE', null, statusObject, callbacks, false);
    }
}
