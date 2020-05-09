import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";

export default class SettingsService extends HCServiceBase
{
    public GetSettings(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<GetSettingsModel> | null = null
    ) : void
    {
        let url = this.options.GetSettingsEndpoint;
        this.fetchExt<GetSettingsModel>(url, 'GET', null, statusObject, callbacks);
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
        
        let url = this.options.SetSettingsEndpoint;
        let payload = {
            settings: settings
        };
        this.fetchExt<any>(url, 'POST', payload, statusObject, callbacks);
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
