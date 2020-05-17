import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";
import { GetEventNotificationConfigsViewModel, EventSinkNotificationConfig } from "../models/EventNotifications/EventNotificationModels";

export default class EventNotificationService extends HCServiceBase
{
    public GetEventNotifications(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<GetEventNotificationConfigsViewModel> | null = null
    ) : void
    {
        let url = this.options.GetEventNotificationConfigsEndpoint;
        this.fetchExt<GetEventNotificationConfigsViewModel>(url, 'GET', null, statusObject, callbacks);
    }
    
    public SetConfigEnabled(config: EventSinkNotificationConfig, enabled: boolean,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<any> | null = null
    ): void {
        let url = this.options.SetEventNotificationConfigEnabledEndpoint;
        let payload = {
            configId: config.Id,
            enabled: enabled
        };
        this.fetchExt<any>(url, 'POST', payload, statusObject, callbacks);
    }

    public SaveConfig(config: EventSinkNotificationConfig,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<EventSinkNotificationConfig> | null = null
    ): void
    {
        let url = this.options.SaveEventNotificationConfigEndpoint;
        let payload = {
            config: config
        };
        this.fetchExt<EventSinkNotificationConfig>(url, 'POST', payload, statusObject, callbacks);
    }

    public DeleteConfig(configId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<EventSinkNotificationConfig> | null = null
    ): void
    {
        let url = this.options.DeleteEventNotificationConfigEndpoint;
        let payload = {
            configId: configId
        };
        this.fetchExt<EventSinkNotificationConfig>(url, 'POST', payload, statusObject, callbacks);
    }

    public DeleteDefintion(eventId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<boolean> | null = null
    ): void
    {
        let url = this.options.DeleteEventDefinitionEndpoint;
        let payload = {
            eventId: eventId
        };
        this.fetchExt<boolean>(url, 'POST', payload, statusObject, callbacks);
    }

    public DeleteDefintions(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<boolean> | null = null
    ): void
    {
        let url = this.options.DeleteEventDefinitionsEndpoint;
        this.fetchExt<boolean>(url, 'POST', null, statusObject, callbacks);
    }
}
