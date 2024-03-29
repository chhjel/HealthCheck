//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { TestResultStatus } from '../../Enums/Core/TestResultStatus';
import { TestResultDataDumpViewModel } from './TestResultDataDumpViewModel';

export interface TestResultViewModel
{
	TestId: string;
	TestName: string;
	StatusCode: number;
	Status: TestResultStatus;
	Message: string;
	StackTrace: string;
	ExpandDataByDefault: boolean;
	AllowExpandData: boolean;
	DisplayClean: boolean;
	ParameterFeedback: { [key:number]: string };
	Data: TestResultDataDumpViewModel[];
	DurationInMilliseconds: number;
	InputWasAllowedAuditLogged: boolean;
	ResultMessageWasAllowedAuditLogged: boolean;
}
