import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";
import { GetEventNotificationConfigsViewModel, EventSinkNotificationConfig } from "../models/EventNotifications/EventNotificationModels";

export default class EventNotificationService extends HCServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    public GetEventNotifications(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<GetEventNotificationConfigsViewModel> | null = null
    ) : void
    {
        this.invokeModuleMethod(this.moduleId, 'GetEventNotificationConfigs', null, statusObject, callbacks);
    }
    
    public SetConfigEnabled(config: EventSinkNotificationConfig, enabled: boolean,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<any> | null = null
    ): void {
        let payload = {
            ConfigId: config.Id,
            Enabled: enabled
        };
        this.invokeModuleMethod(this.moduleId, 'SetEventNotificationConfigEnabled', payload, statusObject, callbacks);
    }

    public SaveConfig(config: EventSinkNotificationConfig,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<EventSinkNotificationConfig> | null = null
    ): void
    {
        this.invokeModuleMethod(this.moduleId, 'SaveEventNotificationConfig', config, statusObject, callbacks);
    }

    public DeleteConfig(configId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<EventSinkNotificationConfig> | null = null
    ): void
    {
        this.invokeModuleMethod(this.moduleId, 'DeleteEventNotificationConfig', configId, statusObject, callbacks);
    }

    public DeleteDefintion(eventId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<boolean> | null = null
    ): void
    {
        this.invokeModuleMethod(this.moduleId, 'DeleteEventDefinition', eventId, statusObject, callbacks);
    }

    public DeleteDefintions(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<boolean> | null = null
    ): void
    {
        this.invokeModuleMethod(this.moduleId, 'DeleteEventDefinitions', null, statusObject, callbacks);
    }
}
