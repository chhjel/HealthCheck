//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { TestResult } from './TestResult';

export interface ProxyRuntimeTestConfig
{
	TargetClassType: any;
	InstanceFactory: () => any;
	InstanceFactoryWithContext: (arg: any) => any;
	ContextFactory: () => any;
	ResultAction: (arg: TestResult, arg1: any) => void;
}