//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { HCMappedMemberReferenceDefinition } from './HCMappedMemberReferenceDefinition';

export interface HCMappedMemberDefinition
{
	Id: string;
	PropertyName: string;
	FullPropertyPath: string;
	DisplayName: string;
	Member: any;
	Remarks: string;
	IsValid: boolean;
	Error: string;
	Parent: HCMappedMemberDefinition;
	Children: HCMappedMemberDefinition[];
	MappedTo: HCMappedMemberReferenceDefinition[];
}
