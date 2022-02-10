//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { EditorWorkerConfig } from './EditorWorkerConfig';
import { HCLoginWebAuthnMode } from '../../Enums/WebUI/HCLoginWebAuthnMode';
import { HCLoginTwoFactorCodeInputMode } from '../../Enums/WebUI/HCLoginTwoFactorCodeInputMode';
import { HCIntegratedProfileConfig } from './HCIntegratedProfileConfig';
import { HCUserModuleCategories } from '../Core/HCUserModuleCategories';

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
	IntegratedLoginCurrent2FACodeExpirationTime: Date;
	IntegratedLogin2FACodeLifetime: number;
	IntegratedLoginSend2FACodeEndpoint: string;
	IntegratedLogin2FANote: string;
	IntegratedLoginSend2FACodeButtonText: string;
	IntegratedLoginWebAuthnNote: string;
	IntegratedLoginWebAuthnMode: HCLoginWebAuthnMode;
	IntegratedLoginTwoFactorCodeInputMode: HCLoginTwoFactorCodeInputMode;
	IntegratedProfileConfig: HCIntegratedProfileConfig;
	AllowAccessTokenKillswitch: boolean;
	UserRoles: string[];
	UserModuleCategories: HCUserModuleCategories[];
}
