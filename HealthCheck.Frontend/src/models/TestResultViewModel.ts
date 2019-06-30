import TestResultDataDumpViewModel from "./TestResultDataDumpViewModel";

export default interface TestResultViewModel {
    TestId: string;
    StatusCode: number;
    Message: string;
    Data: Array<TestResultDataDumpViewModel>;
}