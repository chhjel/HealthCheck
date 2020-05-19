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
        let url = this.options.GetTestsEndpoint;
        this.fetchExt<TestsDataViewModel>(url, 'GET', null, statusObject, callbacks);

        this.invokeModuleMethod('TestModuleA', 'TestAsync', 123, null, {
            onSuccess: (d) => console.log(d)
        });
        // let asd = this.invokeModuleMethod(this.config.Id, '');
    }

    public CancelTest(testId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<any> | null = null
    ) : void
    {
        let url = `${this.options.CancelTestEndpoint}?testId=${testId}`;
        this.fetchExt<any>(url, 'POST', null, statusObject, callbacks, false);
    }

    public ExecuteTest(payload: ExecuteTestPayload,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TestResultViewModel> | null = null
    ) : void
    {
        let url = this.options.ExecuteTestEndpoint;
        this.fetchExt<TestResultViewModel>(url, 'POST', payload, statusObject, callbacks);
    }
}
