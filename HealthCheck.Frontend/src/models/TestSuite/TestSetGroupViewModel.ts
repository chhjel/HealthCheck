import TestSetViewModel from "./TestSetViewModel";

export default interface TestSetGroupViewModel {
    Id: string;
    Sets: Array<TestSetViewModel>;
    Name: string | null;
    UIOrder: number;
}