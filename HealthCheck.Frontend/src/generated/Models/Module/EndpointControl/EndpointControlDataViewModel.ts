//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { EndpointControlRule } from './EndpointControlRule';
import { EndpointControlEndpointDefinition } from './EndpointControlEndpointDefinition';
import { EndpointControlCustomResultDefinitionViewModel } from './EndpointControlCustomResultDefinitionViewModel';
import { HCEndpointControlConditionDefinitionViewModel } from './HCEndpointControlConditionDefinitionViewModel';

export interface EndpointControlDataViewModel
{
	Rules: EndpointControlRule[];
	EndpointDefinitions: EndpointControlEndpointDefinition[];
	CustomResultDefinitions: EndpointControlCustomResultDefinitionViewModel[];
	Conditions: HCEndpointControlConditionDefinitionViewModel[];
}
