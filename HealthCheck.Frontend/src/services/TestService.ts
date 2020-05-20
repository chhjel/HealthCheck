import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";
import TestsDataViewModel from "../models/TestSuite/TestsDataViewModel";
import TestResultViewModel from "../models/TestSuite/TestResultViewModel";
import ExecuteTestPayload from "../models/TestSuite/ExecuteTestPayload";
import ModuleConfig from "../models/Common/ModuleConfig";
import FrontEndOptionsViewModel from "../models/Common/FrontEndOptionsViewModel";

export default class TestService extends HCServiceBase
{
    public moduleId: string;

    constructor(options: FrontEndOptionsViewModel, moduleId: string)
    {
        super(options);
        this.moduleId = moduleId;
    }

    public GetTests(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TestsDataViewModel> | null = null
    ) : void
    {
        this.invokeModuleMethod(this.moduleId, 'GetTests', null, statusObject, callbacks);
    }

    public CancelTest(testId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<any> | null = null
    ) : void
    {
        this.invokeModuleMethod(this.moduleId, 'CancelTest', testId, statusObject, callbacks);
    }

    public ExecuteTest(payload: ExecuteTestPayload,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TestResultViewModel> | null = null
    ) : void
    {
        this.invokeModuleMethod(this.moduleId, 'ExecuteTest', payload, statusObject, callbacks);
    }
}
