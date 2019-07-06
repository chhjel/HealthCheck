import TestSetViewModel from "./TestSetViewModel";
import GroupOptionsViewModel from "./GroupOptionsViewModel";

export default interface TestsDataViewModel {
    TestSets: Array<TestSetViewModel>;
    GroupOptions: Array<GroupOptionsViewModel>;
}