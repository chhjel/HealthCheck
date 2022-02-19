import UrlUtils from 'util/UrlUtils';
import { HCDataExportDeleteStreamQueryPresetsRequest } from './../generated/Models/Module/DataExport/HCDataExportDeleteStreamQueryPresetsRequest';
import { HCDataExportStreamQueryPresetViewModel } from './../generated/Models/Module/DataExport/HCDataExportStreamQueryPresetViewModel';
import { HCDataExportQueryRequest } from "generated/Models/Module/DataExport/HCDataExportQueryRequest";
import { HCDataExportQueryResponseViewModel } from "generated/Models/Module/DataExport/HCDataExportQueryResponseViewModel";
import { HCDataExportSaveStreamQueryPresetRequest } from "generated/Models/Module/DataExport/HCDataExportSaveStreamQueryPresetRequest";
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
    
    public PrepareExport(
        payload: HCDataExportQueryRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<string | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "PrepareExport", payload, statusObject, callbacks);
    }
    
    public GetStreamQueryPresets(
        streamId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<HCDataExportStreamQueryPresetViewModel> | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetStreamQueryPresets", streamId, statusObject, callbacks);
    }
    
    public SaveStreamQueryPreset(
        payload: HCDataExportSaveStreamQueryPresetRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCDataExportStreamQueryPresetViewModel | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "SaveStreamQueryPreset", payload, statusObject, callbacks);
    }
    
    public DeleteStreamQueryPreset(
        payload: HCDataExportDeleteStreamQueryPresetsRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "DeleteStreamQueryPresets", payload, statusObject, callbacks, false);
    }

    public CreateExportDownloadUrl(endpointBase: string, key: string): string {
        let url = `${endpointBase}/DEExport/${key}`;
        const token = UrlUtils.GetQueryStringParameter('x-token');
        if (token != null)
        {
            url += `?x-token=${token}`;
        }
        return url;
    }
}
