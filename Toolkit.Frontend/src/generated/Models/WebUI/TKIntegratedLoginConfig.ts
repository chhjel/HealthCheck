//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { TKLoginTwoFactorCodeConfig } from './TKLoginTwoFactorCodeConfig';
import { TKLoginWebAuthnConfig } from './TKLoginWebAuthnConfig';

export interface TKIntegratedLoginConfig
{
	IntegratedLoginEndpoint: string;
	TwoFactorCodeConfig: TKLoginTwoFactorCodeConfig;
	WebAuthnConfig: TKLoginWebAuthnConfig;
}
