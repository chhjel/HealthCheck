//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { HCDataRepeaterSimpleLogEntry } from './HCDataRepeaterSimpleLogEntry';

export interface HCDataRepeaterStreamItemViewModel
{
	Id: string;
	InsertedAt: Date;
	LastRetriedAt: Date;
	LastRetryWasSuccessful: boolean;
	LastActionAt: Date;
	LastActionWasSuccessful: boolean;
	ItemId: string;
	Summary: string;
	AllowRetry: boolean;
	Tags: string[];
	InitialError: string;
	Log: HCDataRepeaterSimpleLogEntry[];
	SerializedData: string;
	SerializedDataOverride: string;
}