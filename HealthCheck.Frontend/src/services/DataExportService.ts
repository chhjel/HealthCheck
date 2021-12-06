import { HCDataExportQueryRequest } from "generated/Models/Module/DataExport/HCDataExportQueryRequest";
import { HCDataExportQueryResponseViewModel } from "generated/Models/Module/DataExport/HCDataExportQueryResponseViewModel";
import { HCGetDataExportStreamDefinitionsViewModel } from "generated/Models/Module/DataExport/HCGetDataExportStreamDefinitionsViewModel";
import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";

export default class DataExportService extends HCServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    public GetStreamDefinitions(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCGetDataExportStreamDefinitionsViewModel | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetStreamDefinitions", null, statusObject, callbacks);
    }
    
    public QueryStreamPaged(
        payload: HCDataExportQueryRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCDataExportQueryResponseViewModel | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "QueryStreamPaged", payload, statusObject, callbacks);
    }
}
