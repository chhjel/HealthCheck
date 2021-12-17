//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { HCDataExportValueFormatterConfig } from './HCDataExportValueFormatterConfig';

export interface HCDataExportQueryRequest
{
	StreamId: string;
	PageIndex: number;
	PageSize: number;
	Query: string;
	PresetId?: string;
	IncludedProperties: string[];
	HeaderNameOverrides: { [key:string]: string };
	ExporterId: string;
	CustomParameters: { [key:string]: string };
	ValueFormatterConfigs: { [key:string]: HCDataExportValueFormatterConfig };
}
