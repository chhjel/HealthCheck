//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { HCBackendInputConfig } from './HCBackendInputConfig';

export interface HCDataRepeaterStreamActionViewModel
{
	Id: string;
	Name: string;
	Description: string;
	ExecuteButtonLabel: string;
	ParameterDefinitions: HCBackendInputConfig[];
	AllowedOnItemsWithTags: string[];
}
