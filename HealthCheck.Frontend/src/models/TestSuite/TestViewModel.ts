import TestParameterViewModel from "./TestParameterViewModel";
import TestResultViewModel from "./TestResultViewModel";

export default interface TestViewModel {
    Id: string;
    Name: string;
    Description: string;
    RunButtonText: string;
    RunningButtonText: string;
    Parameters: Array<TestParameterViewModel>;

    // Extra data
    TestResult: TestResultViewModel | null;
}