import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";
import FrontEndOptionsViewModel from "../models/Common/FrontEndOptionsViewModel";

export default class SettingsService extends HCServiceBase
{
    public moduleId: string;

    constructor(options: FrontEndOptionsViewModel, moduleId: string)
    {
        super(options);
        this.moduleId = moduleId;
    }
    
    public GetSettings(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<GetSettingsModel> | null = null
    ) : void
    {
        this.invokeModuleMethod(this.moduleId, 'GetSettings', null, statusObject, callbacks);
    }
    
    public SaveSettings(
        data: Array<CustomSettingGroup>,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<any> | null = null
    ) : void
    {
        let settings = data
                .map(x => x.settings)
                .reduce((a: CustomSetting[], b: CustomSetting[]) => a.concat(b))
                .map((x: CustomSetting) => {
                    return {
                        Id: x.id,
                        Value: x.value
                    };
                });
        
        let payload = {
            settings: settings
        };
        this.invokeModuleMethod(this.moduleId, 'SetSettings', payload, statusObject, callbacks, false);
    }
}

export interface GetSettingsModel {
    Settings: Array<BackendSetting>;
}
export interface BackendSetting
{
    Id: string;
    DisplayName: string;
    Description: string | null;
    Type: 'Boolean' | 'String' | 'Int32';
    Value: any;
    GroupName: string | null;
}

export interface CustomSetting
{
    id: string;
    displayName: string;
    description: string | null;
    type: 'Boolean' | 'String' | 'Int32';
    value: any;
    validationError: string | null;
}

export interface CustomSettingGroup
{
    name: string | null;
    settings: Array<CustomSetting>;
}
