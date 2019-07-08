import TestParameterViewModel from "./TestParameterViewModel";

export default interface TestViewModel {
    Id: string;
    Name: string;
    Description: string;
    Parameters: Array<TestParameterViewModel>;
}