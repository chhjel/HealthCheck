//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { TKTestTiming } from './TKTestTiming';
import { TestResult } from './TestResult';

export interface TKTestContext
{
	TestExecutionStartTime: Date;
	Timings: TKTestTiming[];
	TestResultActions: (arg: TestResult) => void[];
}