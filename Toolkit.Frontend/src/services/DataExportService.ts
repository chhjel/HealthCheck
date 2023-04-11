import UrlUtils from '@util/UrlUtils';
import { TKDataExportDeleteStreamQueryPresetsRequest } from './../generated/Models/Module/DataExport/TKDataExportDeleteStreamQueryPresetsRequest';
import { TKDataExportStreamQueryPresetViewModel } from './../generated/Models/Module/DataExport/TKDataExportStreamQueryPresetViewModel';
import { TKDataExportQueryRequest } from "@generated/Models/Module/DataExport/TKDataExportQueryRequest";
import { TKDataExportQueryResponseViewModel } from "@generated/Models/Module/DataExport/TKDataExportQueryResponseViewModel";
import { TKDataExportSaveStreamQueryPresetRequest } from "@generated/Models/Module/DataExport/TKDataExportSaveStreamQueryPresetRequest";
import { TKGetDataExportStreamDefinitionsViewModel } from "@generated/Models/Module/DataExport/TKGetDataExportStreamDefinitionsViewModel";
import TKServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/TKServiceBase";

export default class DataExportService extends TKServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    public GetStreamDefinitions(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKGetDataExportStreamDefinitionsViewModel | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetStreamDefinitions", null, statusObject, callbacks);
    }
    
    public QueryStreamPaged(
        payload: TKDataExportQueryRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKDataExportQueryResponseViewModel | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "QueryStreamPaged", payload, statusObject, callbacks, true, true);
    }
    
    public PrepareExport(
        payload: TKDataExportQueryRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<string | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "PrepareExport", payload, statusObject, callbacks, true, true);
    }
    
    public GetStreamQueryPresets(
        streamId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<TKDataExportStreamQueryPresetViewModel> | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetStreamQueryPresets", streamId, statusObject, callbacks);
    }
    
    public SaveStreamQueryPreset(
        payload: TKDataExportSaveStreamQueryPresetRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKDataExportStreamQueryPresetViewModel | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "SaveStreamQueryPreset", payload, statusObject, callbacks, true, true);
    }
    
    public DeleteStreamQueryPreset(
        payload: TKDataExportDeleteStreamQueryPresetsRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "DeleteStreamQueryPresets", payload, statusObject, callbacks, false);
    }

    public CreateExportDownloadUrl(endpointBase: string, key: string): string {
        let url = `${location.origin}${endpointBase}/DEExport/${key}`;
        url = UrlUtils.ApplyCurrentQueryStringParametersTo(url);
        return url;
    }
}
