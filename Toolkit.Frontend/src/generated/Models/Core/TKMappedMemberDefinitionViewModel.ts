//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { TKMappedMemberReferenceDefinitionViewModel } from './TKMappedMemberReferenceDefinitionViewModel';

export interface TKMappedMemberDefinitionViewModel
{
	Id: string;
	PropertyName: string;
	PropertyTypeName: string;
	FullPropertyTypeName: string;
	FullPropertyPath: string;
	DisplayName: string;
	Remarks: string;
	IsValid: boolean;
	Error: string;
	Children: TKMappedMemberDefinitionViewModel[];
	MappedTo: TKMappedMemberReferenceDefinitionViewModel[];
}