//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

export interface EndpointControlEndpointRequestData
{
	Timestamp: Date;
	EndpointName: string;
	EndpointId: string;
	UserLocationId: string;
	UserAgent: string;
	HttpMethod: string;
	ControllerType: any;
	ControllerName: string;
	ActionName: string;
	Url: string;
	WasBlocked: boolean;
	BlockingRuleId: string;
	HttpContext: any;
}
