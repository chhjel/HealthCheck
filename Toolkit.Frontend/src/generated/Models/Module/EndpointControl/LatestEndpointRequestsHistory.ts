//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { LatestUserEndpointRequestHistory } from './LatestUserEndpointRequestHistory';
import { EndpointRequestDetails } from './EndpointRequestDetails';

export interface LatestEndpointRequestsHistory
{
	LatestRequestIdentities: string[];
	IdentityRequests: { [key:string]: LatestUserEndpointRequestHistory };
	LatestRequests: EndpointRequestDetails[];
}
