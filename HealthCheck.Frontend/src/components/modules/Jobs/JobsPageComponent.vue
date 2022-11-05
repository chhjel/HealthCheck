<!-- src/components/modules/Jobs/JobsPageComponent.vue -->
<template>
    <div class="jobs">

        <div>
            <div v-for="job in jobDefinitions"
                :key="`job-${job.SourceId}-${job.Definition.Id}`">
                <h4>{{ job.Definition.Name }}</h4>
                <i>{{ job.Definition.Description }}</i>
                <btn-component v-if="job.Definition.SupportsStart"
                    :disabled="isLoading"
                    @click="startJob(job.SourceId, job.Definition.Id)"
                    >Start</btn-component>
                <btn-component v-if="job.Definition.SupportsStart"
                    :disabled="isLoading"
                    @click="stopJob(job.SourceId, job.Definition.Id)"
                    >Stop</btn-component>
            </div>
        </div>

        <!-- <code>
            {{jobDefinitions}}
        </code> -->
        <code>
            {{latestHistory}}
        </code>
        <code>
            {{jobStatuses}}
        </code>
        <code>
            {{pagedHistory}}
        </code>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import FilterableListComponent from '@components/Common/FilterableListComponent.vue';
import { FetchStatus } from '@services/abstractions/HCServiceBase';
import ModuleConfig from '@models/Common/ModuleConfig';
import ModuleOptions from '@models/Common/ModuleOptions';
import { StoreUtil } from "@util/StoreUtil";
import JobsService from "@services/JobsService";
import IdUtils from "@util/IdUtils";
import { HCJobDefinitionWithSourceViewModel } from "@generated/Models/Core/HCJobDefinitionWithSourceViewModel";
import { HCJobHistoryEntryViewModel } from "@generated/Models/Core/HCJobHistoryEntryViewModel";
import { HCJobHistoryDetailEntryViewModel } from "@generated/Models/Core/HCJobHistoryDetailEntryViewModel";
import { HCPagedJobHistoryEntryViewModel } from "@generated/Models/Core/HCPagedJobHistoryEntryViewModel";
import { HCJobStatusViewModel } from "@generated/Models/Core/HCJobStatusViewModel";

@Options({
    components: {
        FilterableListComponent
    }
})
export default class JobsPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<any>;

    // Service
    service: JobsService = new JobsService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();

    id: string = IdUtils.generateId();
    jobDefinitions: Array<HCJobDefinitionWithSourceViewModel> = [];
    latestHistory: Array<HCJobHistoryEntryViewModel> = [];
    historyDetails: HCJobHistoryDetailEntryViewModel | null = null;
    jobStatuses: Array<HCJobStatusViewModel> = [];
    pagedHistory: HCPagedJobHistoryEntryViewModel | null = null;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        this.loadJobDefinitions();
        this.loadLatestHistory();
        this.loadJobStatuses();
    }

    ////////////////
    //  METHODS  //
    //////////////
    loadJobDefinitions(): void {
        this.service.GetJobDefinitions(this.dataLoadStatus, {
            onSuccess: (data) => this.onJobDefinitionsRetrieved(data)
        });
    }

    onJobDefinitionsRetrieved(data: Array<HCJobDefinitionWithSourceViewModel> | null): void {
        this.jobDefinitions = data || [];
    }

    loadLatestHistory(): void {
        this.service.GetLatestHistoryPerJobId(this.dataLoadStatus, {
            onSuccess: (data) => this.latestHistory = data || []
        });
    }

    loadPagedHistory(jobId: string, pageIndex: number, pageSize: number): void {
        this.service.GetPagedHistory(jobId, pageIndex, pageSize, this.dataLoadStatus, {
            onSuccess: (data) => this.pagedHistory = data
        });
    }

    loadHistoryDetails(id: string): void {
        this.service.GetHistoryDetail(id, this.dataLoadStatus, {
            onSuccess: (data) => this.historyDetails = data
        });
    }

    loadJobStatuses(): void {
        this.service.GetJobStatuses(this.dataLoadStatus, {
            onSuccess: (data) => this.jobStatuses = data
        });
    }

    loadJobStatus(sourceId: string, jobId: string): void {
        this.service.GetJobStatus(sourceId, jobId, this.dataLoadStatus, {
            onSuccess: (data) => console.log(data)
        });
    }

    startJob(sourceId: string, jobId: string): void {
        this.service.StartJob(sourceId, jobId, this.dataLoadStatus, {
            onSuccess: (data) => console.log(data)
        });
    }

    stopJob(sourceId: string, jobId: string): void {
        this.service.StopJob(sourceId, jobId, this.dataLoadStatus, {
            onSuccess: (data) => console.log(data)
        });
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }

    get isLoading(): boolean {
        return this.dataLoadStatus.inProgress;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
}
</script>

<style scoped lang="scss">
.jobs {

}
</style>