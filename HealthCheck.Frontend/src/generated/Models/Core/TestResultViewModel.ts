//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { TestResultStatus } from '../../Enums/Core/TestResultStatus';
import { TestResultDataDumpViewModel } from './TestResultDataDumpViewModel';

export interface TestResultViewModel
{
	testId: string;
	testName: string;
	statusCode: number;
	status: TestResultStatus;
	message: string;
	stackTrace: string;
	expandDataByDefault: boolean;
	allowExpandData: boolean;
	displayClean: boolean;
	data: TestResultDataDumpViewModel[];
	durationInMilliseconds: number;
}
