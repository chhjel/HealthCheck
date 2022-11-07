<!-- src/components/modules/Jobs/JobsOverviewComponent.vue -->
<template>
    <div class="jobs-overview-component">
        <div v-for="group in groupedjobDefinitions"
            :key="`group-${group.name}`">
            <div v-if="group.name">{{ group.name }}</div>
            <div v-for="job in group.jobs"
                :key="`job-${job.job.SourceId}-${job.job.Definition.Id}`">
                <div class="job-title">{{ job.job.Definition.Name }}</div>
                <div class="job-description">{{ job.job.Definition.Description }}</div>
                <div class="job-status" v-if="job.status">{{ createStatusSummary(job.status) }}</div>
                <div class="job-history" v-if="job.latestHistory">{{ createHistorySummary(job.latestHistory) }}</div>
                <div class="job-icon">
                    <icon-component>{{ jobIcon(job) }}</icon-component>
                </div>
            </div>
        </div>
        <!-- <code>jobDefinitions:{{jobDefinitions}}</code> -->
        <!-- <code>latestHistoryPerJob:{{latestHistoryPerJob}}</code> -->
        <!-- <code>jobStatuses:{{jobStatuses}}</code> -->
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import { FetchStatus } from '@services/abstractions/HCServiceBase';
import ModuleConfig from '@models/Common/ModuleConfig';
import ModuleOptions from '@models/Common/ModuleOptions';
import { StoreUtil } from "@util/StoreUtil";
import JobsService from "@services/JobsService";
import IdUtils from "@util/IdUtils";
import { HCJobDefinitionWithSourceViewModel } from "@generated/Models/Core/HCJobDefinitionWithSourceViewModel";
import { HCJobStatusViewModel } from "@generated/Models/Core/HCJobStatusViewModel";
import { HCJobHistoryEntryViewModel } from "@generated/Models/Core/HCJobHistoryEntryViewModel";
import DateUtils from "@util/DateUtils";
import { HCJobHistoryStatus } from "@generated/Enums/Core/HCJobHistoryStatus";

interface DefGroup {
    name: string;
    jobs: Array<JobDetails>;
}
interface JobDetails {
    job: HCJobDefinitionWithSourceViewModel;
    status: HCJobStatusViewModel | null;
    latestHistory: HCJobHistoryEntryViewModel | null;
}

@Options({
    components: {
    }
})
export default class JobsOverviewComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<any>;
    
    @Prop({ required: true })
    jobDefinitions: Array<HCJobDefinitionWithSourceViewModel>;

    @Prop({ required: true })
    jobStatuses: Array<HCJobStatusViewModel>;
    
    // Service
    service: JobsService = new JobsService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();

    id: string = IdUtils.generateId();
    latestHistoryPerJob: Array<HCJobHistoryEntryViewModel> = [];

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        this.loadLatestHistoryPerJob();
    }

    ////////////////
    //  METHODS  //
    //////////////
    loadLatestHistoryPerJob(): void {
        this.service.GetLatestHistoryPerJobId(this.dataLoadStatus, {
            onSuccess: (data) => this.latestHistoryPerJob = data || []
        });
    }

    getJobStatus(sourceId: string, jobId: string): HCJobStatusViewModel | null {
        return this.jobStatuses.find(x => x.SourceId == sourceId && x.JobId == jobId);
    }

    getJobHistory(sourceId: string, jobId: string): HCJobHistoryEntryViewModel | null {
        return this.latestHistoryPerJob.find(x => x.SourceId == sourceId && x.JobId == jobId);
    }

    createJobDetails(job: HCJobDefinitionWithSourceViewModel): JobDetails {
        return {
            job: job,
            status: this.getJobStatus(job.SourceId, job.Definition.Id),
            latestHistory: this.getJobHistory(job.SourceId, job.Definition.Id),
        };
    }

    createHistorySummary(history: HCJobHistoryEntryViewModel): string {
        const timestamp = new Date(history.Timestamp);
        const time = DateUtils.FormatDate(timestamp, 'd. MMM HH:mm:ss');
        return `${time}: ${history.Summary}`;
    }

    jobIcon(job: JobDetails): string {
        if (job.status?.IsRunning === true) return 'sync';
        else if (job.latestHistory?.Status === HCJobHistoryStatus.Warning) return 'warning';
        else if (job.latestHistory?.Status === HCJobHistoryStatus.Error) return 'error';
        else if (job.status?.LastRunWasSuccessful === false) return 'error';
        else if (job.status?.LastRunWasSuccessful === true) return 'check_circle';
        else if (job.latestHistory?.Status === HCJobHistoryStatus.Success) return 'check_circle';
        else if (job.status?.LastRunWasSuccessful === false) return 'error';
        else if (job.status?.IsEnabled === false) return 'block';
        else return '';
    }

    createStatusSummary(status: HCJobStatusViewModel): string {
        let timestamp = null;
        const start = status.StartedAt == null ? null : new Date(status.StartedAt);
        const end = status.EndedAt == null ? null : new Date(status.EndedAt);
        if (start != null && end != null) timestamp = start.getTime() > end.getTime() ? start : end;
        else timestamp = end || start;

        const time = timestamp == null ? '' : `${DateUtils.FormatDate(timestamp, 'd. MMM HH:mm:ss')}: `;
        return `${time}${status.Summary}`;
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
    
    get groupedjobDefinitions(): Array<DefGroup> {
        const groups: Array<DefGroup> = [];
        const ungrouped = this.jobDefinitions.filter(x => !x.Definition.GroupName);
        if (ungrouped.length > 0) {
            groups.push({
                name: '',
                jobs: ungrouped.map(j => this.createJobDetails(j))
            });
        }

        this.jobDefinitions
            .filter(x => x.Definition.GroupName)
            .forEach(x => {
                let group = groups.find(g => g.name == x.Definition.GroupName);
                if (!group) {
                    group = { name: x.Definition.GroupName, jobs: [] };
                    groups.push(group);
                }
                group.jobs.push(this.createJobDetails(x));
            });
        return groups;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
}
</script>

<style scoped lang="scss">
.jobs-overview-component {

}
</style>
