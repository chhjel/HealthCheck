import TestSetViewModel from "./TestSetViewModel";

export default interface TestSetGroupViewModel {
    Sets: Array<TestSetViewModel>;
    Name: string | null;
    Icon: string | null;
    UIOrder: number | null;
}