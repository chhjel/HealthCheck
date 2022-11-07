<!-- src/components/modules/Jobs/JobsPageComponent.vue -->
<template>
    <div class="jobs">
        <!-- NAVIGATION DRAWER -->
        <Teleport to="#module-nav-menu">
            <div>
                <side-menu-list-component
                    :items="topMenuItems"
                    :disabled="isLoading"
                    ref="sideMenuList"
                    class="mt-5"
                    v-on:itemClicked="onSideMenuItemClicked"
                    @itemMiddleClicked="onSideMenuItemMiddleClicked"
                    />
            </div>

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
                :noTopMargin="true"
                ref="filterableList"
                v-on:itemClicked="onMenuItemClicked"
                @itemMiddleClicked="onMenuItemMiddleClicked"
                />
        </Teleport>

        <div class="content-root">
            <jobs-overview-component
                v-if="!selectedJob"
                :config="config"
                :options="options"
                :jobDefinitions="jobDefinitions"
                :jobStatuses="jobStatuses"
                />

            <job-component
                v-if="selectedJob"
                :config="config"
                :options="options"
                :job="selectedJob"
                :status="selectedJobStatus"
                :key="`${selectedJob.SourceId}_${selectedJob.Definition.Id}`"
                @updateJobRunningStatus="updateJobRunningStatus"
                />
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
import { HCJobStatusViewModel } from "@generated/Models/Core/HCJobStatusViewModel";
import StringUtils from "@util/StringUtils";
import { FilterableListItem } from "@components/Common/FilterableListComponent.vue.models";
import HashUtils from "@util/HashUtils";
import UrlUtils from "@util/UrlUtils";
import JobComponent from "./JobComponent.vue";
import JobsOverviewComponent from "./JobsOverviewComponent.vue";
import { SideMenuListItem } from "@components/Common/SideMenuListComponent.vue.models";
import SideMenuListComponent from "@components/Common/SideMenuListComponent.vue";

@Options({
    components: {
        FilterableListComponent,
        JobComponent,
        JobsOverviewComponent,
        SideMenuListComponent
    }
})
export default class JobsPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<any>;
    
    @Ref() readonly filterableList!: FilterableListComponent;
    @Ref() readonly sideMenuList!: SideMenuListComponent;

    // Service
    service: JobsService = new JobsService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();

    id: string = IdUtils.generateId();
    jobDefinitions: Array<HCJobDefinitionWithSourceViewModel> = [];
    selectedJob: HCJobDefinitionWithSourceViewModel | null = null;
    statusUpdateTimout: NodeJS.Timeout;
    jobStatuses: Array<HCJobStatusViewModel> = [];
    topMenuItems: Array<SideMenuListItem> = [
        {
            label: 'Overview',
            id: 'overview',
            icon: 'view_list',
            data: () => this.selectedJob = null
        }
    ];

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        this.loadJobDefinitions();
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
            const matchingMenuItem = this.topMenuItems.filter(x => this.hash(x.id) == idFromHash)[0];
            if (matchingMenuItem) {
                this.setActiveJob(matchingMenuItem.id, false);
                return;
            }

            const matchingJob = this.jobDefinitions.filter(x => this.hash(x.Definition.Id) == idFromHash)[0];
            if (matchingJob) {
                this.setActiveJob(matchingJob, false);
                return;
            }

            this.setActiveJob(this.topMenuItems[0].id);
        }
    }

    loadJobStatuses(): void {
        this.service.GetJobStatuses(null, {
            onSuccess: (data) => this.jobStatuses = data
        });
    }

    loadJobStatus(sourceId: string, jobId: string): void {
        this.service.GetJobStatus(sourceId, jobId, this.dataLoadStatus, {
            onSuccess: (data) => console.log(data)
        });
    }
    
    updateJobRunningStatus(sourceId: string, jobId: string, success: boolean, running: boolean, message: string): void {
        if (!success) {
            alert(message);
            return;
        }

        var status = this.jobStatuses.find(x => x.SourceId == sourceId && x.JobId == jobId);
        if (!status) {
            status = {
                SourceId: sourceId,
                JobId: jobId,
                Summary: '',
                IsRunning: running,
                IsEnabled: false,
                StartedAt: new Date(),
                EndedAt: null,
                LastRunWasSuccessful: null,
                NextExecutionScheduledAt: null
            };
            this.jobStatuses.push(status);
        }

        status.StartedAt = new Date();
        status.Summary = message;
        status.IsRunning = running;
    }

    setActiveJob(job: HCJobDefinitionWithSourceViewModel | string | null, updateUrl: boolean = true): void {
        if (this.isLoading) {
            return;
        }

        let idForUrl: string = '';
        if (typeof job === 'string') {
            idForUrl = job;
            this.sideMenuList.setSelectedItemById(job);
            (this.filterableList as FilterableListComponent).setSelectedItem(null);
            const menuItem = this.topMenuItems.find(x => x.id == job);
            menuItem?.data();
        }
        else
        {
            this.selectedJob = job;
            (this.filterableList as FilterableListComponent).setSelectedItem(job);
            this.sideMenuList.setSelectedItemById(null);
            if (job == null)
            {
                return;
            }
            idForUrl = job.Definition.Id;
        }

        if (updateUrl && StringUtils.stringOrFirstOfArray(this.$route.params.streamId) != StringUtils.stringOrFirstOfArray(this.hash(idForUrl)))
        {
            this.$router.push(`/jobs/${this.hash(idForUrl)}`);
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

    get selectedJobStatus(): HCJobStatusViewModel | null {
        if (this.selectedJob == null) return null;
        return this.jobStatuses.find(x => x.SourceId == this.selectedJob.SourceId && x.JobId == this.selectedJob.Definition.Id);
    }
    
    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onMenuItemClicked(item: FilterableListItem): void {
        this.setActiveJob(item.data);
    }

    onMenuItemMiddleClicked(item: FilterableListItem): void {
        if (item && item.data && item.data.Definition && item.data.Definition.Id)
        {
            const idHash = this.hash(item.data.Definition.Id);
            const route = `#/jobs/${idHash}`;
            UrlUtils.openRouteInNewTab(route);
        }
    }

    onSideMenuItemClicked(item: SideMenuListItem): void {
        this.setActiveJob(item.id, true);
    }

    onSideMenuItemMiddleClicked(item: SideMenuListItem): void {
        if (item && item.id)
        {
            const idHash = this.hash(item.id);
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