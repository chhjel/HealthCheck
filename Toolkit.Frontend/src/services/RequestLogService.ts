import TKServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/TKServiceBase";
import LoggedEndpointDefinitionViewModel from "../models/modules/RequestLog/LoggedEndpointDefinitionViewModel";
import FrontEndOptionsViewModel from "../models/Common/FrontEndOptionsViewModel";

export default class RequestLogService extends TKServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    public GetRequestLog(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<LoggedEndpointDefinitionViewModel>> | null = null
    ) : void
    {
        this.invokeModuleMethod(this.moduleId, 'GetRequestLog', null, statusObject, callbacks);
    }

    public ClearRequestLog(includeDefinitions: boolean,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<any> | null = null
    ): void
    {
        this.invokeModuleMethod(this.moduleId, 'ClearRequestLog', includeDefinitions, statusObject, callbacks);
    }
}
