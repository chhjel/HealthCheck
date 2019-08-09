import TestViewModel from "./TestViewModel";

export default interface TestSetViewModel {
    Id: string;
    Name: string;
    Description: string;
    GroupName: string| null;
    UIOrder: number;
    AllowRunAll: boolean;
    Tests: Array<TestViewModel>;
}
