//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

export interface HCIntegratedLoginConfig
{
	IntegratedLoginEndpoint: string;
	Show2FAInput: boolean;
	Send2FACodeEndpoint: string;
	Send2FACodeButtonText: string;
	Current2FACodeExpirationTime: Date;
	TwoFactorCodeLifetime: number;
}