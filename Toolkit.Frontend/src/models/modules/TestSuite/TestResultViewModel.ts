import TestResultDataDumpViewModel from "./TestResultDataDumpViewModel";

export default interface TestResultViewModel {
    TestId: string;
    TestName: string;
    StatusCode: number;
    Message: string;
    StackTrace: string;
    AllowExpandData: boolean;
	ParameterFeedback: { [key:number]: string };
    DisplayClean: boolean;
    ExpandDataByDefault: boolean;
    DurationInMilliseconds: number;
    Data: Array<TestResultDataDumpViewModel>;
}