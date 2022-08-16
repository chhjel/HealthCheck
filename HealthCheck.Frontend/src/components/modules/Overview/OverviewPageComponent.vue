<!-- src/components/modules/Overview/OverviewPageComponent.vue -->
<template>
    <div>
        <div class="content-root overview-page-content">
            <!-- LOAD ERROR -->
            <alert-component
                :value="loadStatus.failed"
                type="error">
            {{ loadStatus.errorMessage }}
            </alert-component>

            <!-- PROGRESS BAR -->
            <progress-linear-component
                v-if="loadStatus.inProgress"
                indeterminate color="success"></progress-linear-component>
            
            <!-- SUMMARY -->
            <div v-if="showContent" class="mb-4" >
                <div class="header-layout">
                    <h1 class="header-layout__title">Current status</h1>
                    <div class="header-layout__actions">
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
            
                        <btn-component @click="deleteAllDialogVisible = true"
                            v-if="canDeleteEvents"
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

                <status-component :type="summaryType" :text="summaryText" v-if="summaryText && showTopStatus" />

                <site-events-summary-component
                    v-if="currentEvents.length > 0 && showOngoingEvents"
                    :events="currentEvents"
                    v-on:eventClicked="showEventDetailsDialog" />
            </div>

            <!-- TIMELINE -->
            <div v-if="showContent && showRecentEvents" class="mb-4">
                <h2>Recent events</h2>
                <event-timeline-component
                    :events="timelineEvents"
                    v-on:eventClicked="showEventDetailsDialog"
                    class="timeline" />
            </div>

            <!-- CALENDAR -->
            <div v-if="showContent && showCalendar">
                <h2 class="mb-2">History</h2>
                <event-calendar-component
                    :events="calendarEvents"
                    :initialMode="initialCalendarMode"
                    :allowedModes="allowedCalendarModes"
                    v-on:eventClicked="showEventDetailsDialog"
                    class="calendar" />
            </div>
        </div>

        <!-- ##################### -->
        <!-- ###### DIALOGS ######-->
        <dialog-component v-model:value="eventDetailsDialogState" width="700"
            :headerColor="getEventSeverityColorClass(selectedEventForDetails?.Severity)">
            <template #header v-if="selectedEventForDetails != null">
                <icon-component>{{ getEventSeverityIcon(selectedEventForDetails.Severity) }}</icon-component>
                <div class="details-title">{{ selectedEventForDetails.Title }}</div>
            </template>
            <template #headerRight v-if="selectedEventForDetails != null">
                <div class="details-date">
                    {{ getEventTimeLine1(selectedEventForDetails) }}<br />{{ getEventTimeLine2(selectedEventForDetails) }}
                </div>
            </template>
            <template #footer>
                <btn-component @click="showDeleteSingleDialog(selectedEventForDetails)"
                    v-if="canDeleteEvents"
                    :loading="deleteStatus.inProgress"
                    :disabled="deleteStatus.inProgress"
                    color="error">
                    <icon-component size="20px" class="mr-2">clear</icon-component>
                    Delete
                </btn-component>
                <btn-component color="secondary" @click="eventDetailsDialogState = false">
                    Close
                </btn-component>
            </template>

            <site-event-details-component :event="selectedEventForDetails" v-if="selectedEventForDetails != null">
            </site-event-details-component>
        </dialog-component>
        <!-- ##################### -->
        <dialog-component v-model:value="deleteAllDialogVisible" max-width="500">
            <template #header>Confirm delete</template>
            <template #footer>
                <btn-component color="error" @click="clearAllEvents">Clear all</btn-component>
                <btn-component color="secondary" @click="deleteAllDialogVisible = false">Cancel</btn-component>
            </template>
            <div>
                Clear all site events?
            </div>
        </dialog-component>
        <!-- ##################### -->
        <dialog-component v-model:value="deleteSingleDialogVisible" max-width="550">
            <template #header>{{ deleteSingleDialogTitle }}</template>
            <template #footer>
                <btn-component color="error" @click="deleteSingleEvent">Delete</btn-component>
                <btn-component color="secondary" @click="deleteSingleDialogVisible = false">Cancel</btn-component>
            </template>
            <div>
                {{ deleteSingleDialogText }}
            </div>
        </dialog-component>
        <!-- DIALOGS END -->
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
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
import { HCSiteEventsModuleFrontendOptionsModel } from "@generated/Models/Core/HCSiteEventsModuleFrontendOptionsModel";
import { StoreUtil } from "@util/StoreUtil";
import InputComponent from "@components/Common/Basic/InputComponent.vue";
import { HCSiteEventsModuleCalendarMode } from "@generated/Enums/Core/HCSiteEventsModuleCalendarMode";

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
    options!: ModuleOptions<HCSiteEventsModuleFrontendOptionsModel>;

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
    
    initialCalendarMode: string = '';
    allowedCalendarModes: Array<string> | null = null;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    created(): void {
        if (this.options.Options.FrontendAutoRefreshSecondsInterval < 5)
        {
            this.options.Options.FrontendAutoRefreshSecondsInterval = 5;
        }
        this.initialCalendarMode = this.translateCalendarMode(this.options.Options?.Sections?.Calendar?.InitialMode);
        this.allowedCalendarModes = (this.options.Options?.Sections?.Calendar?.AllowedModes || []).map(x => this.translateCalendarMode(x));
    }

    translateCalendarMode(mode: HCSiteEventsModuleCalendarMode | null | undefined): string {
        if (mode == HCSiteEventsModuleCalendarMode.Month) return 'dayGridMonth';
        else if (mode == HCSiteEventsModuleCalendarMode.Week) return 'timeGridWeek';
        else if (mode == HCSiteEventsModuleCalendarMode.Day) return 'timeGridDay';
        else if (mode == HCSiteEventsModuleCalendarMode.List) return 'listWeek';
        else return '';
    }

    mounted(): void
    {
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

    get showTopStatus(): boolean {
        return this.options.Options.Sections?.Status?.Enabled == true;
    }

    get showOngoingEvents(): boolean {
        return this.options.Options.Sections?.OngoingEvents?.Enabled == true;
    }

    get showRecentEvents(): boolean {
        return this.options.Options.Sections?.RecentEvents?.Enabled == true;
    }

    get showCalendar(): boolean {
        return this.options.Options.Sections?.Calendar?.Enabled == true;
    }

    get performFiltering(): boolean { return this.filter != ''; }
    
    get calendarEvents(): Array<SiteEventViewModel> {
        return this.siteEvents
            .filter(x => this.isEventMatchingFilter(x));
    }

    get timelineEvents(): Array<SiteEventViewModel> {
        const maxDays = this.options.Options.Sections?.RecentEvents?.MaxNumberOfDays || 3;
        let fromDate = new Date();
        fromDate.setDate(fromDate.getDate() - maxDays);
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
        thresholdDate.setMinutes(thresholdDate.getMinutes() - this.options.Options.Sections?.OngoingEvents?.BufferMinutes || 0);
        
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

        // More than a single of highest severity
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
        // Has some events
        else if (relevantEvents.length >= 1) {
            if (severity == SiteEventSeverity.Information) {
                return "An informative event has been reported";
            }
            else if (severity == SiteEventSeverity.Warning) {
                return "A warning has been reported";
            }
            else if (severity == SiteEventSeverity.Error) {
                return "An error is currently ongoing";
            }
            else if (severity == SiteEventSeverity.Fatal) {
                return "The site is currently experiencing an error";
            }
        }
        return null;//relevantMessages[0];
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

    getEventSeverityColorClass(severity: SiteEventSeverity): string {
        if (severity == SiteEventSeverity.Information) {
            return 'info';
        } else if (severity == SiteEventSeverity.Warning) {
            return 'warning';
        } else if (severity == SiteEventSeverity.Error) {
            return 'error';
        } else if (severity == SiteEventSeverity.Fatal) {
            return 'fatal';
        } else {
            return '';
        }
    }

    getEventSeverityIcon(severity: SiteEventSeverity): string {
        if (severity == SiteEventSeverity.Information) {
            return 'info';
        } else if (severity == SiteEventSeverity.Warning) {
            return 'warning';
        } else if (severity == SiteEventSeverity.Error) {
            return 'error';
        } else if (severity == SiteEventSeverity.Fatal) {
            return 'report';
        } else {
            return '';
        }
    }

    getEventTimeLine1(event: SiteEventViewModel) : string {
        if (event.Timestamp.getDate() === event.EndTime.getDate()) {
            let dateFormat = 'dd. MMMM';
            return DateUtils.FormatDate(event.Timestamp, dateFormat);
        } else {
            let timeFormat = 'HH:mm';
            let dateFormat = 'dd. MMM';
            return `${DateUtils.FormatDate(event.Timestamp, `${dateFormat} ${timeFormat}`)} -`;
        }
    }

    getEventTimeLine2(event: SiteEventViewModel) : string {
        let timeFormat = 'HH:mm';
        if (event.Timestamp.getDate() === event.EndTime.getDate()) {
            let start = event.Timestamp;
            let end = this.getEventEndDate(event);
            if (end == null) {
                return DateUtils.FormatDate(start, timeFormat);
            } else {
                return `${DateUtils.FormatDate(start, timeFormat)} - ${DateUtils.FormatDate(end, timeFormat)}`;
            }
        } else {
            let dateFormat = 'dd. MMM';
            return `${DateUtils.FormatDate(event.EndTime, `${dateFormat} ${timeFormat}`)}`;
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
    }
}
.details-title {
    flex: 2;
    margin-left: 5px;
}
.details-date {
    font-size: 12px;
    /* color: var(--color--accent-darken9); */
    text-align: right;
    margin-left: 10px;
}
</style>
