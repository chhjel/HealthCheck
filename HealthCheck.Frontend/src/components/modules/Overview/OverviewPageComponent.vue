<!-- src/components/modules/Overview/OverviewPageComponent.vue -->
<template>
    <div>
        <div> <!-- PAGE-->
        <div fluid fill-height class="content-root">
        <div>
        <div class="pl-4 pr-4 pb-4 overview-page-content">
          <!-- CONTENT BEGIN -->
            
        <div grid-list-md>
            <div align-content-center wrap>
                <!-- LOAD ERROR -->
                <alert-component
                    :value="loadStatus.failed"
                    type="error">
                {{ loadStatus.errorMessage }}
                </alert-component>

                <!-- PROGRESS BAR -->
                <progress-linear-component
                    v-if="loadStatus.inProgress"
                    indeterminate color="green"></progress-linear-component>
                
                <!-- SUMMARY -->
                <div sm12 v-if="showContent" class="mb-4" >
                    <div style="display: flex">
                        <h1 class="mb-2" style="flex: 1">Current status</h1>
                        
                        <btn-component flat v-if="options.Options.ShowFilter && !showFilter" @click="showFilter = !showFilter">Filter data..</btn-component>
                        <btn-component flat v-if="options.Options.ShowFilter && showFilter" @click="showFilter = !showFilter">Hide filter</btn-component>

                        <btn-component @click="toggleAutoRefresh"
                            :loading="deleteStatus.inProgress"
                            :disabled="deleteStatus.inProgress"
                            color="secondary"
                            flat class="mr-0">
                            <icon-component class="mr-2" v-if="!autoRefreshEnabled">update_disabled</icon-component>
                            <progress-circular-component
                                v-if="autoRefreshEnabled"
                                :size="20"
                                :width="3"
                                :value="autoRefreshValue"
                                color="primary"
                                style="margin-right: 10px;"
                                ></progress-circular-component>
                            Auto-refresh
                        </btn-component>
            
                        <div v-if="canDeleteEvents">
                            <btn-component @click="deleteAllDialogVisible = true"
                                :loading="deleteStatus.inProgress"
                                :disabled="deleteStatus.inProgress || !siteEvents || siteEvents.length == 0"
                                flat color="error" class="mr-0">
                                <icon-component class="mr-2">clear</icon-component>
                                Delete all
                            </btn-component>
                        </div>
                    </div>
                
                    <!-- CUSTOM HTML -->
                    <div class="mb-2 mt-2" v-if="options.Options.CustomHtml" v-html="options.Options.CustomHtml"></div>

                    <!-- FILTER -->
                    <div v-if="options.Options.ShowFilter" class="mb-2">
                        <input-component v-if="showFilter" v-model:value="filterInternal" name="Filter" />
                    </div>

                    <status-component :type="summaryType" :text="summaryText" />

                    <site-events-summary-component
                        v-if="currentEvents.length > 0"
                        :events="currentEvents"
                        v-on:eventClicked="showEventDetailsDialog" />
                </div>

                <!-- TIMELINE -->
                <div sm12 v-if="showContent" class="mb-4">
                    <h2>Past events</h2>
                    <event-timeline-component
                        :events="timelineEvents"
                        v-on:eventClicked="showEventDetailsDialog"
                        class="timeline" />
                </div>

                <!-- CALENDAR -->
                <div sm12 v-if="showContent">
                    <h2>History</h2>
                    <event-calendar-component
                        :events="calendarEvents"
                        v-on:eventClicked="showEventDetailsDialog"
                        class="calendar" />
                </div>
            </div>
        </div>

          <!-- CONTENT END -->
        </div>
        </div>
        </div>
        </div> <!-- /PAGE-->

        <!-- ##################### -->
        <!-- ###### DIALOGS ######-->
        <dialog-component v-model:value="eventDetailsDialogState" width="700">
            <site-event-details-component :event="selectedEventForDetails" v-if="selectedEventForDetails != null">
                <template v-slot:actions>
                    <btn-component flat color="secondary" @click="eventDetailsDialogState = false">
                        Close
                    </btn-component>
                    <btn-component @click="showDeleteSingleDialog(selectedEventForDetails)"
                        v-if="canDeleteEvents"
                        :loading="deleteStatus.inProgress"
                        :disabled="deleteStatus.inProgress"
                        flat color="error">
                        <icon-component size="20px" class="mr-2">clear</icon-component>
                        Delete
                    </btn-component>
                </template>
            </site-event-details-component>
        </dialog-component>
        <!-- ##################### -->
        <dialog-component v-model:value="deleteAllDialogVisible"
            @keydown.esc="deleteAllDialogVisible = false"
            max-width="350">
            <card-component>
                <div class="headline">Confirm deletion</div>
                <div>
                    Clear all site events?
                </div>
                                <div>
                                        <btn-component color="primary" @click="deleteAllDialogVisible = false">Cancel</btn-component>
                    <btn-component color="error" @click="clearAllEvents">Clear all</btn-component>
                </div>
            </card-component>
        </dialog-component>
        <!-- ##################### -->
        <dialog-component v-model:value="deleteSingleDialogVisible"
            @keydown.esc="deleteSingleDialogVisible = false"
            max-width="550">
            <card-component>
                <div class="headline">{{ deleteSingleDialogTitle }}</div>
                <div>
                    {{ deleteSingleDialogText }}
                </div>
                                <div>
                                        <btn-component color="primary" @click="deleteSingleDialogVisible = false">Cancel</btn-component>
                    <btn-component color="error" @click="deleteSingleEvent">Delete</btn-component>
                </div>
            </card-component>
        </dialog-component>
        <!-- DIALOGS END -->
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import CalendarEvent from '@models/Common/CalendarEvent';
import EventTimelineComponent from '@components/modules/Overview/EventTimelineComponent.vue';
import EventCalendarComponent from '@components/modules/Overview/EventCalendarComponent.vue';
import SiteEventDetailsComponent from '@components/modules/Overview/SiteEventDetailsComponent.vue';
import SiteEventsSummaryComponent from '@components/modules/Overview/SiteEventsSummaryComponent.vue';
import StatusComponent from '@components/modules/Overview/StatusComponent.vue';
import DateUtils from '@util/DateUtils';
import LinqUtils from '@util/LinqUtils';
import OverviewService from '@services/OverviewService';
import { FetchStatus } from '@services/abstractions/HCServiceBase';
import ModuleOptions from '@models/Common/ModuleOptions';
import ModuleConfig from '@models/Common/ModuleConfig';
import SiteEventViewModel from "@models/modules/SiteEvents/SiteEventViewModel";
import { SiteEventSeverity } from "@models/modules/SiteEvents/SiteEventSeverity";
import { SiteEvent } from "@generated/Models/Core/SiteEvent";
import { StoreUtil } from "@util/StoreUtil";
import InputComponent from "@components/Common/Basic/InputComponent.vue";

interface OverviewPageOptions
{
    CurrentEventBufferMinutes: number;
    FrontendAutoRefreshSecondsInterval: number;
    CustomHtml: string;
    ShowFilter: boolean;
}

@Options({
    components: {
        EventTimelineComponent,
        EventCalendarComponent,
        SiteEventDetailsComponent,
        SiteEventsSummaryComponent,
        StatusComponent,
        InputComponent
    }
})
export default class OverviewPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<OverviewPageOptions>;

    // Dialogs
    eventDetailsDialogState: boolean = false;
    selectedEventForDetails: SiteEventViewModel | null = null;
    
    // Service
    service: OverviewService = new OverviewService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    loadStatus: FetchStatus = new FetchStatus();

    deleteAllDialogVisible: boolean = false;
    deleteSingleDialogVisible: boolean = false;
    targetIdToDelete: string = '';
    deleteSingleDialogText: string = '';
    deleteSingleDialogTitle: string = '';
    deleteStatus: FetchStatus = new FetchStatus();

    siteEvents: Array<SiteEventViewModel> = [];
    autoRefreshEnabled: boolean = false;
    autoRefreshRef: any = 0;
    autoRefreshValue: number = 100;
    nextAutoRefresh: Date | null = null;
    filterInternal: string = '';
    showFilter: boolean = false;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        if (this.options.Options.FrontendAutoRefreshSecondsInterval < 5)
        {
            this.options.Options.FrontendAutoRefreshSecondsInterval = 5;
        }
        setInterval(() => { this.autoRefreshValueCalc(); }, 1000);

        this.loadData();
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }

    get filter(): string {
        if (!this.showFilter) return '';
        else return this.filterInternal || '';
    }

    get performFiltering(): boolean { return this.filter != ''; }
    
    get calendarEvents(): Array<SiteEventViewModel> {
        return this.siteEvents
            .filter(x => this.isEventMatchingFilter(x));
    }

    get timelineEvents(): Array<SiteEventViewModel> {
        let fromDate = new Date();
        fromDate.setDate(fromDate.getDate() - 3);
        fromDate.setHours(23);
        fromDate.setMinutes(59);
        
        return this.siteEvents
            .filter(x => new Date(x.EndTime) >= fromDate)
            .filter(x => this.isEventMatchingFilter(x));
    }

    get currentEvents(): Array<SiteEventViewModel> {
        return this.siteEvents
            .filter(x => this.isEventCurrent(x))
            .filter(x => this.isEventMatchingFilter(x))
            .sort((a, b) => LinqUtils.SortByThenBy(a, b,
                (item) => item.SeverityCode,
                (item) => item.Timestamp)
            );
    }

    isEventMatchingFilter(event: SiteEventViewModel): boolean {
        if (!this.performFiltering) return true;
        else return(
            event.EventTypeId?.toLowerCase()?.includes(this.filter.toLowerCase())
            || event.Severity?.toLowerCase()?.includes(this.filter.toLowerCase())
            || event.Title?.toLowerCase()?.includes(this.filter.toLowerCase())
            || event.Description?.toLowerCase()?.includes(this.filter.toLowerCase())
            || event.DeveloperDetails?.toLowerCase()?.includes(this.filter.toLowerCase())
            || event.Timestamp?.toString()?.toLowerCase()?.includes(this.filter.toLowerCase())
            );
    }

    isEventCurrent(event: SiteEventViewModel): boolean {
        if (event.Resolved) {
            return false;
        }

        let thresholdDate = new Date();
        thresholdDate.setMinutes(thresholdDate.getMinutes() - this.options.Options.CurrentEventBufferMinutes);
        
        let eventEndDate = new Date(event.Timestamp);
        eventEndDate.setMinutes(eventEndDate.getMinutes() + event.Duration);
        return eventEndDate.getTime() >= thresholdDate.getTime();
    }

    get showContent(): boolean {
        return !this.loadStatus.inProgress && !this.loadStatus.failed;
    }

    get summaryText(): string {
        let relevantEvents = this.currentEvents;
        let severity = this.getCurrentSeverity();
        if (relevantEvents.length == 0 || severity == null) {
            return "All Systems Operational";
        }

        let relevantMessages = relevantEvents
            .filter(x => x.Severity == severity)
            .map(x => x.Title);

        if (relevantMessages.length > 1) {
            if (severity == SiteEventSeverity.Information) {
                return "Some informative events have been reported";
            }
            else if (severity == SiteEventSeverity.Warning) {
                return "A few warnings have been reported";
            }
            else if (severity == SiteEventSeverity.Error) {
                return "A few errors are currently ongoing";
            }
            else if (severity == SiteEventSeverity.Fatal) {
                return "The site is currently experiencing a few errors";
            }
        }

        return relevantMessages[0];
    }

    get summaryType(): string {
        let severity = this.getCurrentSeverity();
        if (severity == null) {
            return "success";
        }
        else if (severity == SiteEventSeverity.Information) {
            return "info";
        }
        else if (severity == SiteEventSeverity.Warning) {
            return "warning";
        }
        else if (severity == SiteEventSeverity.Error) {
            return "error";
        }
        else if (severity == SiteEventSeverity.Fatal) {
            return "fatal";
        }
        else {
            return "info";
        }
    }

    ////////////////
    //  METHODS  //
    //////////////
    hasAccess(option: string): boolean {
        return this.options.AccessOptions.indexOf(option) != -1;
    }

    get canDeleteEvents(): boolean {
        return this.hasAccess('DeleteEvents');
    }

    loadData(): void {
        this.service.GetSiteEvents(this.loadStatus, { onSuccess: (data) => this.onEventDataRetrieved(data) });
    }
    
    onEventDataRetrieved(events: Array<SiteEventViewModel>): void {
        this.loadStatus.inProgress = false;
        let index = -1;
        events.forEach(x => {
            index++;
            x.Timestamp = new Date(x.Timestamp);
            x.EndTime = new Date(x.EndTime);
            x.ResolvedAt = (x.ResolvedAt != null) ? new Date(x.ResolvedAt) : null;
        });
        this.siteEvents = events;
    }

    clearAllEvents(): void {
        this.deleteAllDialogVisible = false;
        this.service.ClearSiteEvents(this.deleteStatus, {
            onSuccess: () => {
                this.siteEvents = [];
            }
        });
    }

    showDeleteSingleDialog(e: SiteEvent): void {
        this.targetIdToDelete = e.Id;
        this.deleteSingleDialogTitle = `Delete event '${e.Title}'?`;
        this.deleteSingleDialogText = `Confirm that you want to delete the event '${e.Title}'.`;
        this.deleteSingleDialogVisible = true;
    }

    deleteSingleEvent(): void {
        this.deleteSingleDialogVisible = false;
        this.eventDetailsDialogState = false;
        this.service.DeleteSiteEvent(this.targetIdToDelete, this.deleteStatus, {
            onSuccess: () => {
                this.siteEvents = this.siteEvents.filter(x => x.Id != this.targetIdToDelete);
                this.targetIdToDelete = '';
            }
        });
    }

    getCurrentSeverity(): SiteEventSeverity | null {
        let relevantEvents = this.currentEvents;
        if (relevantEvents.length == 0) {
            return null;
        } else if(relevantEvents.some(x => x.Severity == SiteEventSeverity.Fatal)) {
            return SiteEventSeverity.Fatal;
        } else if(relevantEvents.some(x => x.Severity == SiteEventSeverity.Error)) {
            return SiteEventSeverity.Error;
        } else if(relevantEvents.some(x => x.Severity == SiteEventSeverity.Warning)) {
            return SiteEventSeverity.Warning;
        } else if(relevantEvents.some(x => x.Severity == SiteEventSeverity.Information)) {
            return SiteEventSeverity.Information;
        } else {
            return null;
        }
    }

    getEventDateRange(event: SiteEventViewModel) : string {
        let timeFormat = 'HH:mm';
        let start = event.Timestamp;
        let end = this.getEventEndDate(event);
        if (end == null) {
            return DateUtils.FormatDate(start, timeFormat);
        } else {
            return `${DateUtils.FormatDate(start, timeFormat)} - ${DateUtils.FormatDate(end, timeFormat)}`;
        }
    }

    getEventEndDate(event: SiteEventViewModel) : Date | null {
        if (event.Duration > 1) {
            return new Date(event.Timestamp.getTime() + event.Duration * 60000);
        } else {
            return null;
        }
    }

    showEventDetailsDialog(event: SiteEventViewModel): void {
        this.selectedEventForDetails = event;
        this.eventDetailsDialogState = true;
    }

    toggleAutoRefresh(): void {
        this.autoRefreshEnabled = !this.autoRefreshEnabled;

        if (this.autoRefreshEnabled)
        {
            this.autoRefreshValue = 100;
            this.setNextAutoRefreshDate();
            this.autoRefreshRef = setInterval(() => {
                this.setNextAutoRefreshDate();
                this.autoRefreshValue = 100;
                this.loadData();
            }, this.options.Options.FrontendAutoRefreshSecondsInterval * 1000);
        }
        else
        {
            clearInterval(this.autoRefreshRef);
        }
    }

    setNextAutoRefreshDate(): void {
        this.nextAutoRefresh = new Date();
        this.nextAutoRefresh.setSeconds(this.nextAutoRefresh.getSeconds() + this.options.Options.FrontendAutoRefreshSecondsInterval);
    }

    autoRefreshValueCalc(): void {
        if (!this.nextAutoRefresh) return;
        const secondsLeft = (this.nextAutoRefresh.getTime() - new Date().getTime()) / 1000;        
        const percentage = secondsLeft / this.options.Options.FrontendAutoRefreshSecondsInterval;
        const value = Math.floor(percentage * 100);
        this.autoRefreshValue = Math.min(Math.max(value, 0), 100);
    }
}
</script>

<style scoped lang="scss">
.overview-page-content {
    @media (max-width: 540px) {
        padding-left: 0 !important;
        padding-right: 0 !important;
        .container {
            padding-left: 0 !important;
            padding-right: 0 !important;
        }
    }
}
</style>

<style>
</style>