//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { TestParameterViewModel } from './TestParameterViewModel';

export interface TestViewModel
{
	Id: string;
	Name: string;
	Description: string;
	RunButtonText: string;
	RunningButtonText: string;
	IsCancellable: boolean;
	Parameters: TestParameterViewModel[];
}
