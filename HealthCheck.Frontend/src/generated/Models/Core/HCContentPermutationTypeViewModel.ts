//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { HCContentPermutationChoiceViewModel } from './HCContentPermutationChoiceViewModel';
import { HCContentPermutationPropertyDetails } from './HCContentPermutationPropertyDetails';

export interface HCContentPermutationTypeViewModel
{
	Id: string;
	Name: string;
	Description: string;
	Permutations: HCContentPermutationChoiceViewModel[];
	PropertyDetails: { [key:string]: HCContentPermutationPropertyDetails };
}