import HCServiceBase, { FetchStatus } from "./abstractions/HCServiceBase";

export default class HCSettingsService extends HCServiceBase
{
    public GetSettings(
        statusObject: FetchStatus | null = null,
        onSuccess: ((data: GetSettingsModel) => void ) | null,
        onError: ((message: string) => void) | null = null
    ) : void
    {
        let url = this.options.GetSettingsEndpoint;
        this.fetchExt<GetSettingsModel>(url, 'GET', null, statusObject, onSuccess, onError);
    }
    
    public SaveSettings(
        data: Array<CustomSettingGroup>,
        statusObject: FetchStatus | null = null,
        onSuccess: ((data: GetSettingsModel) => void ) | null = null,
        onError: ((message: string) => void) | null = null
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
        this.fetchExt<GetSettingsModel>(url, 'POST', payload, statusObject, onSuccess, onError);
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
