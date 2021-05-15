import { GetMetricsViewModel } from "generated/Models/Core/GetMetricsViewModel";
import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";

export default class MetricsService extends HCServiceBase
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
