//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { TestParameterViewModel } from './TestParameterViewModel';

export interface TestViewModel
{
	id: string;
	name: string;
	description: string;
	runButtonText: string;
	runningButtonText: string;
	isCancellable: boolean;
	parameters: TestParameterViewModel[];
}
