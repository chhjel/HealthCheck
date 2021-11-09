<!-- src/components/modules/Overview/OverviewPageComponent.vue -->
<template>
    <div>
        <v-content class="pl-0">
        <v-container fluid fill-height class="content-root">
        <v-layout>
        <v-flex class="pl-4 pr-4 pb-4">
          <!-- CONTENT BEGIN -->
            
        <v-container grid-list-md>
            <!-- <h1 class="mb-2">Events overview</h1> -->

            <v-layout align-content-center wrap>
                <!-- LOAD ERROR -->
                <v-alert
                    :value="loadStatus.failed"
                    type="error">
                {{ loadStatus.errorMessage }}
                </v-alert>

                <!-- PROGRESS BAR -->
                <v-progress-linear
                    v-if="loadStatus.inProgress"
                    indeterminate color="green"></v-progress-linear>
                
                <!-- SUMMARY -->
                <v-flex sm12 v-if="showContent" class="mb-4" >
                    <div style="display: flex">
                        <h1 class="mb-2" style="flex: 1">Current status</h1>
            
                        <div v-if="canDeleteEvents">
                            <v-btn @click="deleteAllDialogVisible = true"
                                :loading="deleteStatus.inProgress"
                                :disabled="deleteStatus.inProgress || !siteEvents || siteEvents.length == 0"
                                flat color="error" class="mr-0">
                                <v-icon size="20px" class="mr-2">clear</v-icon>
                                Delete all
                            </v-btn>
                        </div>
                    </div>

                    <status-component :type="summaryType" :text="summaryText" />

                    <site-events-summary-component
                        v-if="currentEvents.length > 0"
                        :events="currentEvents"
                        v-on:eventClicked="showEventDetailsDialog" />
                </v-flex>

                <!-- TIMELINE -->
                <v-flex sm12 v-if="showContent" class="mb-4">
                    <h2>Past events</h2>
                    <event-timeline-component
                        :events="timelineEvents"
                        v-on:eventClicked="showEventDetailsDialog"
                        class="timeline" />
                </v-flex>

                <!-- CALENDAR -->
                <v-flex sm12 v-if="showContent">
                    <h2>History</h2>
                    <event-calendar-component
                        :events="calendarEvents"
                        v-on:eventClicked="showEventDetailsDialog"
                        class="calendar" />
                </v-flex>
            </v-layout>
        </v-container>

          <!-- CONTENT END -->
        </v-flex>
        </v-layout>
        </v-container>
        </v-content>

        <!-- ##################### -->
        <!-- ###### DIALOGS ######-->
        <v-dialog v-model="eventDetailsDialogState" width="700">
            <site-event-details-component :event="selectedEventForDetails" v-if="selectedEventForDetails != null">
                <template v-slot:actions>
                    <v-btn flat color="secondary" @click="eventDetailsDialogState = false">
                        Close
                    </v-btn>
                    <v-btn @click="showDeleteSingleDialog(selectedEventForDetails)"
                        v-if="canDeleteEvents"
                        :loading="deleteStatus.inProgress"
                        :disabled="deleteStatus.inProgress"
                        flat color="error">
                        <v-icon size="20px" class="mr-2">clear</v-icon>
                        Delete
                    </v-btn>
                </template>
            </site-event-details-component>
        </v-dialog>
        <!-- ##################### -->
        <v-dialog v-model="deleteAllDialogVisible"
            @keydown.esc="deleteAllDialogVisible = false"
            max-width="350">
            <v-card>
                <v-card-title class="headline">Confirm deletion</v-card-title>
                <v-card-text>
                    Clear all site events?
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="primary" @click="deleteAllDialogVisible = false">Cancel</v-btn>
                    <v-btn color="error" @click="clearAllEvents">Clear all</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
        <!-- ##################### -->
        <v-dialog v-model="deleteSingleDialogVisible"
            @keydown.esc="deleteSingleDialogVisible = false"
            max-width="550">
            <v-card>
                <v-card-title class="headline">{{ deleteSingleDialogTitle }}</v-card-title>
                <v-card-text>
                    {{ deleteSingleDialogText }}
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="primary" @click="deleteSingleDialogVisible = false">Cancel</v-btn>
                    <v-btn color="error" @click="deleteSingleEvent">Delete</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
        <!-- DIALOGS END -->
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import FrontEndOptionsViewModel from  '../../../models/Common/FrontEndOptionsViewModel';
import CalendarEvent from  '../../../models/Common/CalendarEvent';
import SiteEventViewModel from  '../../../models/modules/SiteEvents/SiteEventViewModel';
import { SiteEventSeverity } from  '../../../models/modules/SiteEvents/SiteEventSeverity';
import EventTimelineComponent from '../Overview/EventTimelineComponent.vue';
import EventCalendarComponent from '../Overview/EventCalendarComponent.vue';
import SiteEventDetailsComponent from '../Overview/SiteEventDetailsComponent.vue';
import SiteEventsSummaryComponent from '../Overview/SiteEventsSummaryComponent.vue';
import StatusComponent from '../Overview/StatusComponent.vue';
import DateUtils from  '../../../util/DateUtils';
import LinqUtils from  '../../../util/LinqUtils';
import OverviewService from  '../../../services/OverviewService';
import { FetchStatus } from  '../../../services/abstractions/HCServiceBase';
import ModuleOptions from  '../../../models/Common/ModuleOptions';
import ModuleConfig from  '../../../models/Common/ModuleConfig';
import { SiteEvent } from "generated/Models/Core/SiteEvent";

interface OverviewPageOptions
{
    CurrentEventBufferMinutes: number;
}

@Component({
    components: {
        EventTimelineComponent,
        EventCalendarComponent,
        SiteEventDetailsComponent,
        SiteEventsSummaryComponent,
        StatusComponent
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

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.loadData();
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return this.$store.state.globalOptions;
    }
    
    get calendarEvents(): Array<SiteEventViewModel> {
        return this.siteEvents;
    }

    get timelineEvents(): Array<SiteEventViewModel> {
        let fromDate = new Date();
        fromDate.setDate(fromDate.getDate() - 3);
        fromDate.setHours(23);
        fromDate.setMinutes(59);
        
        return this.siteEvents.filter(x => new Date(x.EndTime) >= fromDate);
    }

    get currentEvents(): Array<SiteEventViewModel> {
        return this.siteEvents
            .filter(x => this.isEventCurrent(x))
            .sort((a, b) => LinqUtils.SortByThenBy(a, b,
                (item) => item.SeverityCode,
                (item) => item.Timestamp)
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
}
</script>

<style scoped>
/* .content-root {
    margin: auto;
    max-width: 800px;
} */
</style>

<style>
</style>