import TestSetViewModel from "./TestSetViewModel";
import GroupOptionsViewModel from "./GroupOptionsViewModel";
import InvalidTestViewModel from "./InvalidTestViewModel";
import TestParameterTemplateViewModel from "./TestParameterTemplateViewModel";

export default interface TestsDataViewModel {
    TestSets: Array<TestSetViewModel>;
    GroupOptions: Array<GroupOptionsViewModel>;
    InvalidTests: Array<InvalidTestViewModel>;
    ParameterTemplateValues: Array<TestParameterTemplateViewModel>;
}