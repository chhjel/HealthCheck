import { TestResultDataDumpType } from "./TestResultDataDumpType";

export default interface TestResultDataDumpViewModel {
    Title: string | null;
    Data: string;
    Type: TestResultDataDumpType;
}