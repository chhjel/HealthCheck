import { HCJobSimpleResult } from './../generated/Models/Core/HCJobSimpleResult';
import { HCJobDeleteAllHistoryForJobRequestModel } from './../generated/Models/Core/HCJobDeleteAllHistoryForJobRequestModel';
import { HCJobsStartJobRequestModel } from '@generated/Models/Core/HCJobsStartJobRequestModel';
import { HCJobsStopJobRequestModel } from '@generated/Models/Core/HCJobsStopJobRequestModel';
import { HCJobsGetJobStatusRequestModel } from '@generated/Models/Core/HCJobsGetJobStatusRequestModel';
import { HCJobStatusViewModel } from '@generated/Models/Core/HCJobStatusViewModel';
import { HCJobHistoryEntryViewModel } from '@generated/Models/Core/HCJobHistoryEntryViewModel';
import { HCJobsGetPagedHistoryRequestModel } from '@generated/Models/Core/HCJobsGetPagedHistoryRequestModel';
import { HCJobsGetHistoryDetailRequestModel } from '@generated/Models/Core/HCJobsGetHistoryDetailRequestModel';
import { HCJobDefinitionWithSourceViewModel } from '@generated/Models/Core/HCJobDefinitionWithSourceViewModel';
import { HCPagedJobHistoryEntryViewModel } from '@generated/Models/Core/HCPagedJobHistoryEntryViewModel';
import { HCJobHistoryDetailEntryViewModel } from '@generated/Models/Core/HCJobHistoryDetailEntryViewModel';
import { HCJobStartResultViewModel } from '@generated/Models/Core/HCJobStartResultViewModel';
import { HCJobStopResultViewModel } from '@generated/Models/Core/HCJobStopResultViewModel';
import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";
import { HCJobDeleteHistoryItemRequestModel } from '@generated/Models/Core/HCJobDeleteHistoryItemRequestModel';

export default class JobsService extends HCServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    public GetJobDefinitions(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<HCJobDefinitionWithSourceViewModel> | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetJobDefinitions", null, statusObject, callbacks);
    }
    
    public GetPagedHistory(
        sourceId: string, jobId: string, pageIndex: number, pageSize: number,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCPagedJobHistoryEntryViewModel | null> | null = null
    ): void {
        const payload: HCJobsGetPagedHistoryRequestModel = {
            SourceId: sourceId,
            JobId: jobId,
            PageIndex: pageIndex,
            PageSize: pageSize
        };
        this.invokeModuleMethod(this.moduleId, "GetPagedHistory", payload, statusObject, callbacks);
    }
    
    public GetLatestHistoryPerJobId(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<HCJobHistoryEntryViewModel> | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetLatestHistoryPerJobId", null, statusObject, callbacks);
    }
    
    public GetHistoryDetail(
        id: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCJobHistoryDetailEntryViewModel | null> | null = null
    ): void {
        const payload: HCJobsGetHistoryDetailRequestModel = {
            Id: id
        };
        this.invokeModuleMethod(this.moduleId, "GetHistoryDetail", payload, statusObject, callbacks);
    }
    
    public GetJobStatuses(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<HCJobStatusViewModel> | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetJobStatuses", null, statusObject, callbacks);
    }
    
    public GetJobStatus(
        sourceId: string, jobId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCJobStatusViewModel | null> | null = null
    ): void {
        const payload: HCJobsGetJobStatusRequestModel = {
            SourceId: sourceId,
            JobId: jobId
        };
        this.invokeModuleMethod(this.moduleId, "GetJobStatus", payload, statusObject, callbacks);
    }
    
    public StartJob(
        sourceId: string, jobId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCJobStartResultViewModel | null> | null = null
    ): void {
        const payload: HCJobsStartJobRequestModel = {
            SourceId: sourceId,
            JobId: jobId
        };
        this.invokeModuleMethod(this.moduleId, "StartJob", payload, statusObject, callbacks);
    }
    
    public StopJob(
        sourceId: string, jobId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCJobStopResultViewModel | null> | null = null
    ): void {
        const payload: HCJobsStopJobRequestModel = {
            SourceId: sourceId,
            JobId: jobId
        };
        this.invokeModuleMethod(this.moduleId, "StopJob", payload, statusObject, callbacks);
    }

    public DeleteAllHistory(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCJobSimpleResult | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "DeleteAllHistory", null, statusObject, callbacks);
    }
    
    public DeleteAllHistoryForJob(
        sourceId: string, jobId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCJobSimpleResult | null> | null = null
    ): void {
        const payload: HCJobDeleteAllHistoryForJobRequestModel = {
            SourceId: sourceId,
            JobId: jobId
        };
        this.invokeModuleMethod(this.moduleId, "DeleteAllHistoryForJob", payload, statusObject, callbacks);
    }
    
    public DeleteHistoryItem(
        id: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCJobSimpleResult | null> | null = null
    ): void {
        const payload: HCJobDeleteHistoryItemRequestModel = {
            Id: id
        };
        this.invokeModuleMethod(this.moduleId, "DeleteHistoryItem", payload, statusObject, callbacks);
    }
}
