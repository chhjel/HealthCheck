import { Dictionary } from './../EventNotifications/EventNotificationModels';

export interface MessagesInboxMetadata {
    Id: string;
    Name: string;
    Description: string;
}

export interface DataWithTotalCount<T> {
    Data: T;
    TotalCount: number;
}

export interface MessageItem {
    Id: string;
    Timestamp: Date;
    Summary: string;
    From: string;
    To: string;
    Body: string;
    BodyIsHtml: boolean;
    AdditionalDetails: Dictionary<string>;
    HasError: boolean;
    ErrorMessage: string | null;
}
