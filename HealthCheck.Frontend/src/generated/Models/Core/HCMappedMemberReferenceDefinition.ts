//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { HCMappedMemberReferencePathItemDefinition } from './HCMappedMemberReferencePathItemDefinition';

export interface HCMappedMemberReferenceDefinition
{
	Success: boolean;
	Error: string;
	Path: string;
	RootType: any;
	RootReferenceId: string;
	Items: HCMappedMemberReferencePathItemDefinition[];
}
