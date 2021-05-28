//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { EditorWorkerConfig } from './EditorWorkerConfig';

export interface HCFrontEndOptions
{
	ApplicationTitle: string;
	ApplicationTitleLink: string;
	EndpointBase: string;
	InvokeModuleMethodEndpoint: string;
	LogoutLinkTitle: string;
	LogoutLinkUrl: string;
	InludeQueryStringInApiCalls: boolean;
	EditorConfig: EditorWorkerConfig;
	ShowIntegratedLogin: boolean;
	IntegratedLoginEndpoint: string;
	IntegratedLoginShow2FA: boolean;
	IntegratedLoginCurrent2FACodeExpirationTime: Date;
	IntegratedLogin2FACodeLifetime: number;
	IntegratedLoginSend2FACodeEndpoint: string;
	IntegratedLoginSend2FACodeButtonText: string;
}
