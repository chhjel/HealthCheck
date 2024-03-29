//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { TKDataExportStreamItemDefinitionViewModel } from './TKDataExportStreamItemDefinitionViewModel';
import { TKBackendInputConfig } from '../../Core/TKBackendInputConfig';
import { TKDataExportValueFormatterViewModel } from './TKDataExportValueFormatterViewModel';

export interface TKDataExportStreamViewModel
{
	Id: string;
	Name: string;
	Description: string;
	GroupName: string;
	ShowQueryInput: boolean;
	AllowAnyPropertyName: boolean;
	ItemDefinition: TKDataExportStreamItemDefinitionViewModel;
	CustomParameterDefinitions: TKBackendInputConfig[];
	ValueFormatters: TKDataExportValueFormatterViewModel[];
}
