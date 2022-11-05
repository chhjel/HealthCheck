<!-- src/components/modules/Jobs/JobsPageComponent.vue -->
<template>
    <div class="jobs">
        <!-- NAVIGATION DRAWER -->
        <Teleport to="#module-nav-menu">
            <filterable-list-component
                :items="menuItems"
                :groupByKey="x => x.Definition.GroupName"
                :sortByKey="x => x.Definition.GroupName"
                :hrefKey="`Href`"
                :filterKeys="[ 'Name', 'Description' ]"
                :loading="dataLoadStatus.inProgress"
                :disabled="isLoading"
                :showFilter="false"
                :groupIfSingleGroup="false"
                ref="filterableList"
                v-on:itemClicked="onMenuItemClicked"
                @itemMiddleClicked="onMenuItemMiddleClicked"
                />
        </Teleport>

        <div class="content-root">
            <div>
                <code>{{selectedJob}}</code>
                <hr />
                <div v-if="pagedHistory">
                    <div v-for="entry in pagedHistory.Items"
                        :key="`historyEntry-${entry.SourceId}-${entry.Id}`">
                        <code
                            class="clickable"
                            @click="loadHistoryDetails(entry.DetailId)"
                            >{{entry}}</code>
                    </div>
                </div>
                <hr />
                <code>historyDetails:{{historyDetails}}</code>
            </div>
            
            <hr />

            <div>
                <div v-for="job in jobDefinitions"
                    :key="`job-${job.SourceId}-${job.Definition.Id}`">
                    <h4>{{ job.Definition.Name }}</h4>
                    <i>{{ job.Definition.Description }}</i>
                    <btn-component v-if="job.Definition.SupportsStart"
                        :disabled="isLoading || isJobRunning(job.SourceId, job.Definition.Id)"
                        @click="startJob(job.SourceId, job.Definition.Id)"
                        >Start</btn-component>
                    <btn-component v-if="job.Definition.SupportsStop"
                        :disabled="isLoading || !isJobRunning(job.SourceId, job.Definition.Id)"
                        @click="stopJob(job.SourceId, job.Definition.Id)"
                        >Stop</btn-component>
                    <span v-if="isJobRunning(job.SourceId, job.Definition.Id)">Running..</span>
                </div>
            </div>

            <!-- <code>
                {{jobDefinitions}}
            </code> -->
            <code>latestHistoryPerJob:{{latestHistoryPerJob}}</code>
            <code>jobStatuses:{{jobStatuses}}</code>
            <code>pagedHistory:{{pagedHistory}}</code>
            
        </div>
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
import StringUtils from "@util/StringUtils";
import { FilterableListItem } from "@components/Common/FilterableListComponent.vue.models";
import HashUtils from "@util/HashUtils";
import UrlUtils from "@util/UrlUtils";

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
    
    @Ref() readonly filterableList!: FilterableListComponent;

    // Service
    service: JobsService = new JobsService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();

    id: string = IdUtils.generateId();
    jobDefinitions: Array<HCJobDefinitionWithSourceViewModel> = [];
    selectedJob: HCJobDefinitionWithSourceViewModel | null = null;
    statusUpdateTimout: NodeJS.Timeout;

    latestHistoryPerJob: Array<HCJobHistoryEntryViewModel> = [];
    jobStatuses: Array<HCJobStatusViewModel> = [];
    pagedHistory: HCPagedJobHistoryEntryViewModel | null = null;
    historyDetails: HCJobHistoryDetailEntryViewModel | null = null;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        this.loadJobDefinitions();
        this.loadLatestHistoryPerJob();
        this.loadJobStatuses();
    
        // if (this.statusUpdateTimout) clearInterval(this.statusUpdateTimout);
        // this.statusUpdateTimout = setInterval(this.loadJobStatuses, 2500);
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

        const idFromHash = StringUtils.stringOrFirstOfArray(this.$route.params.jobId) || null;
        if (this.jobDefinitions)
        {
            const matchingJob = this.jobDefinitions.filter(x => this.hash(x.Definition.Id) == idFromHash)[0];
            if (matchingJob) {
                this.setActiveJob(matchingJob, false);
            } else if (this.jobDefinitions.length > 0) {
                this.setActiveJob(this.jobDefinitions[0]);   
            }
        }
    }

    loadLatestHistoryPerJob(): void {
        this.service.GetLatestHistoryPerJobId(this.dataLoadStatus, {
            onSuccess: (data) => this.latestHistoryPerJob = data || []
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
            onSuccess: (data) => this.updateJobRunningStatus(sourceId, jobId, data.Success, true, data.Message)
        });
    }

    stopJob(sourceId: string, jobId: string): void {
        this.service.StopJob(sourceId, jobId, this.dataLoadStatus, {
            onSuccess: (data) => this.updateJobRunningStatus(sourceId, jobId, data.Success, false, data.Message)
        });
    }

    isJobRunning(sourceId: string, jobId: string): boolean | null {
        return this.jobStatuses.find(x => x.SourceId == sourceId && x.JobId == jobId)?.IsRunning;
    }
    
    updateJobRunningStatus(sourceId: string, jobId: string, success: boolean, running: boolean, message: string): void {
        if (!success) {
            alert(message);
            return;
        }

        var status = this.jobStatuses.find(x => x.SourceId == sourceId && x.JobId == jobId);
        if (!status) {
            status = status;
            this.jobStatuses.push({
                SourceId: sourceId,
                JobId: jobId,
                Status: '',
                IsRunning: running,
                IsEnabled: false,
                StartedAt: new Date(),
                EndedAt: null,
                LastRunWasSuccessful: null,
                NextExecutionScheduledAt: null
            });
        }

        status.StartedAt = new Date();
        status.Status = message;
        status.IsRunning = running;
    }

    setActiveJob(job: HCJobDefinitionWithSourceViewModel | null, updateUrl: boolean = true): void {
        if (this.isLoading) {
            return;
        }

        this.selectedJob = job;
        (this.filterableList as FilterableListComponent).setSelectedItem(job);
        if (job == null)
        {
            return;
        }

        this.pagedHistory = null;
        this.historyDetails = null;
        this.loadPagedHistory(this.selectedJob.Definition.Id, 0, 50);

        if (updateUrl && StringUtils.stringOrFirstOfArray(this.$route.params.streamId) != StringUtils.stringOrFirstOfArray(this.hash(job.Definition.Id)))
        {
            this.$router.push(`/jobs/${this.hash(job.Definition.Id)}`);
        }
    }
    
    hash(input: string) { return HashUtils.md5(input); }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }

    get isLoading(): boolean {
        return this.dataLoadStatus.inProgress;
    }
    
    get menuItems(): Array<FilterableListItem>
    {
        if (!this.jobDefinitions) return [];
        return this.jobDefinitions.map(x => {
            let d = {
                title: x.Definition.Name,
                subtitle: '',
                data: x
                // data: { ...x, ...{ GroupName: x.Definition.GroupName } }
            };
            (<any>d)['Href'] = "/td";
            return d;
        });
    }
    
    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onMenuItemClicked(item: FilterableListItem): void {
        this.setActiveJob(item.data);
    }

    onMenuItemMiddleClicked(item: FilterableListItem): void {
        if (item && item.data && item.data.Id)
        {
            const idHash = this.hash(item.data.Id);
            const route = `#/jobs/${idHash}`;
            UrlUtils.openRouteInNewTab(route);
        }
    }
}
</script>

<style scoped lang="scss">
.jobs {

}
</style>