import TestViewModel from "./TestViewModel";

export default interface TestSetViewModel {
    Id: string;
    Name: string;
    Description: string;
    Tests: Array<TestViewModel>;
}
