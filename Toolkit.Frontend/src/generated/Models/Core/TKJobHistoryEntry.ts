//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { TKJobHistoryStatus } from '../../Enums/Core/TKJobHistoryStatus';

export interface TKJobHistoryEntry
{
	Id: string;
	SourceId: string;
	JobId: string;
	EndedAt: Date;
	DetailId: string;
	Status: TKJobHistoryStatus;
	Summary: string;
	StartedAt: Date;
}