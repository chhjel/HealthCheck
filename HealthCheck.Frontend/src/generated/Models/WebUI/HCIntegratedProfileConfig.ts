//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { ElevateTotpDelegate } from './ElevateTotpDelegate';
import { AddTotpDelegate } from './AddTotpDelegate';
import { RemoveTotpDelegate } from './RemoveTotpDelegate';
import { ElevateWebAuthnDelegate } from './ElevateWebAuthnDelegate';
import { AddWebAuthnDelegate } from './AddWebAuthnDelegate';
import { CreateWebAuthnRegistrationOptionsDelegate } from './CreateWebAuthnRegistrationOptionsDelegate';
import { RemoveWebAuthnDelegate } from './RemoveWebAuthnDelegate';

export interface HCIntegratedProfileConfig
{
	Hide: boolean;
	ShowHealthCheckRoles: boolean;
	Username: string;
	TotpElevationLogic: ElevateTotpDelegate;
	ShowTotpElevation: boolean;
	TotpElevationEnabled: boolean;
	AddTotpLogic: AddTotpDelegate;
	ShowAddTotp: boolean;
	AddTotpEnabled: boolean;
	RemoveTotpLogic: RemoveTotpDelegate;
	ShowRemoveTotp: boolean;
	RemoveTotpEnabled: boolean;
	WebAuthnElevationLogic: ElevateWebAuthnDelegate;
	ShowWebAuthnElevation: boolean;
	WebAuthnElevationEnabled: boolean;
	AddWebAuthnLogic: AddWebAuthnDelegate;
	ShowAddWebAuthn: boolean;
	AddWebAuthnEnabled: boolean;
	CreateWebAuthnRegistrationOptionsLogic: CreateWebAuthnRegistrationOptionsDelegate;
	RemoveWebAuthnLogic: RemoveWebAuthnDelegate;
	ShowRemoveWebAuthn: boolean;
	RemoveWebAuthnEnabled: boolean;
}