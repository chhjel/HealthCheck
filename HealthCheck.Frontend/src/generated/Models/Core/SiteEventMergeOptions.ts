//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { SiteEvent } from './SiteEvent';

export interface SiteEventMergeOptions
{
	AllowEventMerge: boolean;
	MaxMinutesSinceLastEventEnd: number;
	LastEventDurationMultiplier: number;
	EventMerger: (arg: SiteEvent, arg1: SiteEvent) => void;
}
