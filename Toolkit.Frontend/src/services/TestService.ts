import TKServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/TKServiceBase";
import TestsDataViewModel from "../models/modules/TestSuite/TestsDataViewModel";
import TestResultViewModel from "../models/modules/TestSuite/TestResultViewModel";
import ExecuteTestPayload from "../models/modules/TestSuite/ExecuteTestPayload";
import ModuleConfig from "../models/Common/ModuleConfig";
import FrontEndOptionsViewModel from "../models/Common/FrontEndOptionsViewModel";
import { TestParameterReferenceChoiceViewModel } from "../models/modules/TestSuite/TestParameterViewModel";

export default class TestService extends TKServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
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

    public GetReferenceParameterOptions(
        testId: string, parameterIndex: number, filter: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<TestParameterReferenceChoiceViewModel>> | null = null
    ) : void
    {
        const payload = {
            testId: testId,
            parameterIndex: parameterIndex,
            filter: filter
        };
        this.invokeModuleMethod(this.moduleId, 'GetReferenceParameterOptions', payload, statusObject, callbacks);
    }
    
}
