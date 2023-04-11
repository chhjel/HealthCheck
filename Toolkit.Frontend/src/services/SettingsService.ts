import TKServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/TKServiceBase";
import { GetSettingsViewModel } from "@generated/Models/Core/GetSettingsViewModel";
import { SetSettingsViewModel } from '@generated/Models/Core/SetSettingsViewModel';

export default class SettingsService extends TKServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    public GetSettings(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<GetSettingsViewModel> | null = null
    ) : void
    {
        this.invokeModuleMethod(this.moduleId, 'GetSettings', null, statusObject, callbacks);
    }
    
    public SaveSettings(
        payload: SetSettingsViewModel,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<any> | null = null
    ) : void
    {
        this.invokeModuleMethod(this.moduleId, 'SetSettings', payload, statusObject, callbacks, false);
    }
}
