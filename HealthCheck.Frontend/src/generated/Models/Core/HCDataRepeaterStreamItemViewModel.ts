//     This code was generated by a Reinforced.Typings tool. 
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

export interface HCDataRepeaterStreamItemViewModel
{
	Id: string;
	InsertedAt: Date;
	LastRetriedAt: Date;
	LastRetryWasSuccessful: boolean;
	LastActionAt: Date;
	LastActionWasSuccessful: boolean;
	ItemId: string;
	AllowRetry: boolean;
	Tags: string[];
	InitialError: string;
	Log: string[];
	SerializedData: string;
}
