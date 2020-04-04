export interface Dictionary<T> {
    [Key: string]: T;
}

export interface GetEventNotificationConfigsViewModel {
    Notifiers: Array<IEventNotifier>;
    Configs: Array<EventSinkNotificationConfig>;
}

export interface IEventNotifier {
    Id: string;
    Name: string;
    Description: string;
    Options: Array<EventNotifierOptionDefinition>;
}

export interface EventNotifierOptionDefinition {
    Id: string;
    Name: string;
    Description: string;
}

export interface EventSinkNotificationConfig {
    Id: string;
    Enabled: boolean;
    NotificationCountLimit: number | null;
    FromTime: Date | null;
    ToTime: Date | null;
    NotifierConfigs: Array<NotifierConfig>;
    EventIdFilter: EventSinkNotificationConfigFilter;
    PayloadFilters: Array<EventSinkNotificationConfigFilter>;
    LatestResults: Array<string>;
}

export interface NotifierConfig {
    NotifierId: string;
    Options: Dictionary<string>;
}

export interface EventSinkNotificationConfigFilter {
    PropertyName: string;
    Filter: string;
    MatchType: FilterMatchType;
    CaseSensitive: boolean;
}

export enum FilterMatchType {
    Contains = 0,
    Matches,
    RegEx
}
