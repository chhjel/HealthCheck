import TestResultDataDumpViewModel from "./TestResultDataDumpViewModel";

export default interface TestResultViewModel {
    TestId: string;
    TestName: string;
    StatusCode: number;
    Message: string;
    StackTrace: string;
    DurationInMilliseconds: number;
    Data: Array<TestResultDataDumpViewModel>;
}