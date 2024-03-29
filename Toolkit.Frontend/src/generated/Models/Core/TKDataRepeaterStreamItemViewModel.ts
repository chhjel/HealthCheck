//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { TKDataRepeaterSimpleLogEntry } from './TKDataRepeaterSimpleLogEntry';
import { TKDataRepeaterStreamItemStatus } from '../../Enums/Core/TKDataRepeaterStreamItemStatus';
import { TKDataRepeaterStreamItemActionAllowedViewModel } from './TKDataRepeaterStreamItemActionAllowedViewModel';

export interface TKDataRepeaterStreamItemViewModel
{
	Id: string;
	InsertedAt: Date;
	LastRetriedAt?: Date;
	LastUpdatedAt?: Date;
	LastRetryWasSuccessful?: boolean;
	LastActionAt?: Date;
	ItemId: string;
	Summary: string;
	AllowRetry: boolean;
	Tags: string[];
	FirstErrorAt?: Date;
	LastErrorAt?: Date;
	FirstError: string;
	Error: string;
	Log: TKDataRepeaterSimpleLogEntry[];
	SerializedData: string;
	FirstSerializedData: string;
	ExpiresAt?: Date;
	ForcedStatus?: TKDataRepeaterStreamItemStatus;
	ActionValidationResults: TKDataRepeaterStreamItemActionAllowedViewModel[];
}
