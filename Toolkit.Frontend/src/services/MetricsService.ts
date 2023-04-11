import { GetMetricsViewModel } from "@generated/Models/Core/GetMetricsViewModel";
import TKServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/TKServiceBase";

export default class MetricsService extends TKServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    public GetMetrics(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<GetMetricsViewModel | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetMetrics", null, statusObject, callbacks);
    }
}
