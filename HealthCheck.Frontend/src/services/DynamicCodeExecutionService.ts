import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";
import { DynamicCodeExecutionSourceModel, DynamicCodeExecutionResultModel } from "../models/DynamicCodeExecution/Models";

export default class DynamicCodeExecutionService extends HCServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    public ExecuteCode(model: DynamicCodeExecutionSourceModel,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<DynamicCodeExecutionResultModel> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, 'ExecuteCode', model, statusObject, callbacks);
    }
}
