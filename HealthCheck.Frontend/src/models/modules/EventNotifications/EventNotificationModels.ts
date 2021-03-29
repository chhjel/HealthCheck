import { HCBackendInputConfig } from "generated/Models/Core/HCBackendInputConfig";

export interface Dictionary<T> {
    [Key: string]: T;
}

export interface GetEventNotificationConfigsViewModel {
    Notifiers: Array<IEventNotifier>;
    Configs: Array<EventSinkNotificationConfig>;
    KnownEventDefinitions: Array<KnownEventDefinition>;
    Placeholders: Array<string>;
}

export interface IEventNotifier {
    Id: string;
    Name: string;
    Description: string;
    Options: Array<HCBackendInputConfig>;
    Placeholders: Array<string>;
}

export interface EventSinkNotificationConfig {
    Id: string;
    LastChangedBy: string;
    Enabled: boolean;
    NotificationCountLimit: number | null;
    FromTime: Date | null;
    ToTime: Date | null;
    LastChangedAt: Date;
    LastNotifiedAt: Date | null;
    NotifierConfigs: Array<NotifierConfig>;
    EventIdFilter: EventSinkNotificationConfigFilter;
    PayloadFilters: Array<EventSinkNotificationConfigFilter>;
    LatestResults: Array<string>;

    DistinctNotificationKey: string;
    DistinctNotificationCacheDuration: string; // "HH:MM:SS"
}

export interface NotifierConfig {
    NotifierId: string;
    Options: Dictionary<string>;
    Notifier: IEventNotifier | null;
}

export interface NotifierConfigOptionsItem {
    key: string;
    definition: HCBackendInputConfig;
    value: string;
}

export interface EventSinkNotificationConfigFilter {
    PropertyName: string | null;
    Filter: string;
    MatchType: FilterMatchType;
    CaseSensitive: boolean;

    _frontendId: string;
}

export enum FilterMatchType {
    Contains = 'Contains',
    Matches = 'Matches',
    RegEx = 'RegEx'
}

export interface KnownEventDefinition {
    EventId: string;
    PayloadProperties: Array<string>;
    IsStringified: boolean;
}
