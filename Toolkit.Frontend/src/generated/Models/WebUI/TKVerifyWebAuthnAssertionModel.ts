//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { TKAssertionResponse } from './TKAssertionResponse';
import { TKAuthenticationExtensionsClientOutputs } from './TKAuthenticationExtensionsClientOutputs';

export interface TKVerifyWebAuthnAssertionModel
{
	Id: string;
	RawId: string;
	Response: TKAssertionResponse;
	Extensions: TKAuthenticationExtensionsClientOutputs;
}
