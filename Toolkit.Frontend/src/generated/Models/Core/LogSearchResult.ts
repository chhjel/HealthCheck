//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { LogEntrySearchResultItem } from './LogEntrySearchResultItem';
import { LogSearchStatisticsResult } from './LogSearchStatisticsResult';
import { ParsedQuery } from './ParsedQuery';

export interface LogSearchResult
{
	TotalCount: number;
	Count: number;
	PageCount: number;
	CurrentPage: number;
	Items: LogEntrySearchResultItem[];
	GroupedEntries: { [key:string]: LogEntrySearchResultItem[] };
	ColumnNames: string[];
	DurationInMilliseconds: number;
	WasCancelled: boolean;
	Error: string;
	HasError: boolean;
	LowestDate: Date;
	HighestDate: Date;
	Statistics: LogSearchStatisticsResult[];
	StatisticsIsComplete: boolean;
	ParsedQuery: ParsedQuery;
	ParsedExcludedQuery: ParsedQuery;
	ParsedLogPathQuery: ParsedQuery;
	ParsedExcludedLogPathQuery: ParsedQuery;
}