//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { TKSiteEventsModuleStatusOptions } from './TKSiteEventsModuleStatusOptions';
import { TKSiteEventsModuleOngoingEventsOptions } from './TKSiteEventsModuleOngoingEventsOptions';
import { TKSiteEventsModuleRecentEventsOptions } from './TKSiteEventsModuleRecentEventsOptions';
import { TKSiteEventsModuleCalendarOptions } from './TKSiteEventsModuleCalendarOptions';

export interface TKSiteEventsModuleSectionOptions
{
	Status: TKSiteEventsModuleStatusOptions;
	OngoingEvents: TKSiteEventsModuleOngoingEventsOptions;
	RecentEvents: TKSiteEventsModuleRecentEventsOptions;
	Calendar: TKSiteEventsModuleCalendarOptions;
}