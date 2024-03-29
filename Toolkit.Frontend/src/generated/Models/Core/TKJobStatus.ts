//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { TKJobHistoryStatus } from '../../Enums/Core/TKJobHistoryStatus';

export interface TKJobStatus
{
	SourceId: string;
	JobId: string;
	Summary: string;
	IsRunning: boolean;
	IsEnabled: boolean;
	NextExecutionScheduledAt: Date;
	StartedAt: Date;
	EndedAt: Date;
	Status: TKJobHistoryStatus;
}
