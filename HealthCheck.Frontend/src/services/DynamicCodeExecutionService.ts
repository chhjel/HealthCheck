import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";
import { DynamicCodeExecutionSourceModel, DynamicCodeExecutionResultModel, DynamicCodeScript, AutoCompleteData, AutoCompleteRequest } from "../models/modules/DynamicCodeExecution/Models";

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
    
    public ExecuteScriptById(id: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<DynamicCodeExecutionResultModel> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, 'ExecuteScriptById', id, statusObject, callbacks);
    }
    
    public GetScripts(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<DynamicCodeScript>> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, 'GetScripts', null, statusObject, callbacks);
    }
    
    public DeleteScript(id: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<boolean> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, 'DeleteScript', id, statusObject, callbacks);
    }
    
    public AddNewScript(script: DynamicCodeScript,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<DynamicCodeScript> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, 'AddNewScript', script, statusObject, callbacks);
    }
    
    public SaveScriptChanges(script: DynamicCodeScript,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<DynamicCodeScript> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, 'SaveScriptChanges', script, statusObject, callbacks);
    }

    public AutoComplete(request: AutoCompleteRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<AutoCompleteData>> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, 'AutoComplete', request, statusObject, callbacks);
    }
}
