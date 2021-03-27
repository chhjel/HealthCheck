//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { SequenceDiagramStepDirection } from '../../Enums/Core/SequenceDiagramStepDirection';

export interface SequenceDiagramStep
{
	index: number;
	from: string;
	to: string;
	description: string;
	note: string;
	remarks: string;
	remarkNumber: number;
	optionalGroupName: string;
	direction: SequenceDiagramStepDirection;
	branches: string[];
	classNameFrom: string;
	classNameTo: string;
	methodNameFrom: string;
	methodNameTo: string;
}
