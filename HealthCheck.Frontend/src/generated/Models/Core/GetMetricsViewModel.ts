//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { CompiledMetricsCounterData } from './CompiledMetricsCounterData';
import { CompiledMetricsValueData } from './CompiledMetricsValueData';
import { CompiledMetricsNoteData } from './CompiledMetricsNoteData';

export interface GetMetricsViewModel
{
	GlobalCounters: { [key:string]: CompiledMetricsCounterData };
	GlobalValues: { [key:string]: CompiledMetricsValueData };
	GlobalNotes: { [key:string]: CompiledMetricsNoteData };
}
