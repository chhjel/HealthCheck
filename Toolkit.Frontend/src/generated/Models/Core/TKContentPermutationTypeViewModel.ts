//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { TKContentPermutationChoiceViewModel } from './TKContentPermutationChoiceViewModel';
import { TKBackendInputConfig } from './TKBackendInputConfig';

export interface TKContentPermutationTypeViewModel
{
	Id: string;
	Name: string;
	Description: string;
	MaxAllowedContentCount: number;
	DefaultContentCount: number;
	Permutations: TKContentPermutationChoiceViewModel[];
	PropertyConfigs: TKBackendInputConfig[];
}
