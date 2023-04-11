import TestParameterViewModel from "./TestParameterViewModel";
import TestResultViewModel from "./TestResultViewModel";

export default interface TestViewModel {
    Id: string;
    Name: string;
    Description: string;
    RunButtonText: string;
    RunningButtonText: string;
    IsCancellable: boolean;
    Parameters: Array<TestParameterViewModel>;
	Categories: string[];

    // Extra data
    TestResult: TestResultViewModel | null;
}