//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { DataFlowPropertyUIHint } from '../../Enums/Core/DataFlowPropertyUIHint';
import { DataFlowPropertyUIVisibilityOption } from '../../Enums/Core/DataFlowPropertyUIVisibilityOption';

export interface DataFlowPropertyDisplayInfo
{
	propertyName: string;
	displayName: string;
	uiOrder: number;
	uiHint: DataFlowPropertyUIHint;
	visibility: DataFlowPropertyUIVisibilityOption;
	isFilterable: boolean;
	dateTimeFormat: string;
}
