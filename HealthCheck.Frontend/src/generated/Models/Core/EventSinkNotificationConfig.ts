//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { NotifierConfig } from './NotifierConfig';
import { EventSinkNotificationConfigFilter } from './EventSinkNotificationConfigFilter';

export interface EventSinkNotificationConfig
{
	Id: string;
	LastChangedBy: string;
	LastChangedAt: Date;
	LastNotifiedAt: Date;
	Enabled: boolean;
	NotificationCountLimit: number;
	DistinctNotificationKey: string;
	DistinctNotificationCacheDuration: any;
	FromTime: Date;
	ToTime: Date;
	NotifierConfigs: NotifierConfig[];
	EventIdFilter: EventSinkNotificationConfigFilter;
	PayloadFilters: EventSinkNotificationConfigFilter[];
	LatestResults: string[];
	DistinctCacheEnabled: boolean;
}
