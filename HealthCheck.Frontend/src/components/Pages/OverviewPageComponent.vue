<!-- src/components/Pages/OverviewPageComponent.vue -->
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
                    :value="overviewDataLoadFailed"
                    type="error">
                {{ overviewDataFailedErrorMessage }}
                </v-alert>

                <!-- PROGRESS BAR -->
                <v-progress-linear
                    v-if="overviewDataLoadInProgress"
                    indeterminate color="green"></v-progress-linear>
                
                <!-- SUMMARY -->
                <v-flex sm12 v-if="showContent" class="mb-4" >
                    <h1 class="mb-2">Current status</h1>

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

        <!-- DIALOGS -->
        <v-dialog v-model="eventDetailsDialogState" width="700">
            <site-event-details-component :event="selectedEventForDetails" v-if="selectedEventForDetails != null">
                <template v-slot:actions>
                    <v-btn flat color="secondary" @click="eventDetailsDialogState = false">
                        Close
                    </v-btn>
                </template>
            </site-event-details-component>
        </v-dialog>
        <!-- DIALOGS END -->
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import FrontEndOptionsViewModel from '../../models/Page/FrontEndOptionsViewModel';
import CalendarEvent from '../../models/Common/CalendarEvent';
import SiteEventViewModel from '../../models/SiteEvents/SiteEventViewModel';
import { SiteEventSeverity } from '../../models/SiteEvents/SiteEventSeverity';
import EventTimelineComponent from '../Overview/EventTimelineComponent.vue';
import EventCalendarComponent from '../Overview/EventCalendarComponent.vue';
import SiteEventDetailsComponent from '../Overview/SiteEventDetailsComponent.vue';
import SiteEventsSummaryComponent from '../Overview/SiteEventsSummaryComponent.vue';
import StatusComponent from '../Overview/StatusComponent.vue';
import DateUtils from "../../util/DateUtils";
import LinqUtils from "../../util/LinqUtils";

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
    options!: FrontEndOptionsViewModel;

    // Dialogs
    eventDetailsDialogState: boolean = false;
    selectedEventForDetails: SiteEventViewModel | null = null;

    // Loading
    overviewDataLoadInProgress: boolean = false;
    overviewDataLoadFailed: boolean = false;
    overviewDataFailedErrorMessage: string = "";

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
        let thresholdDate = new Date();
        thresholdDate.setMinutes(thresholdDate.getMinutes() - this.options.CurrentEventBufferMinutes);
        
        let eventEndDate = new Date(event.Timestamp);
        eventEndDate.setMinutes(eventEndDate.getMinutes() + event.Duration);
        return eventEndDate.getTime() >= thresholdDate.getTime();
    }

    get showContent(): boolean {
        return !this.overviewDataLoadInProgress && !this.overviewDataLoadFailed;
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
    loadData(): void {
        this.overviewDataLoadInProgress = true;
        this.overviewDataLoadFailed = false;

        let queryStringIfEnabled = this.options.InludeQueryStringInApiCalls ? window.location.search : '';
        let url = `${this.options.GetSiteEventsEndpoint}${queryStringIfEnabled}`;
        fetch(url, {
            credentials: 'include',
            method: "GET",
            headers: new Headers({
                'Content-Type': 'application/json',
                Accept: 'application/json',
            })
        })
        .then(response => response.json())
        // .then(response => new Promise<Array<SiteEventViewModel>>(resolve => setTimeout(() => resolve(response), 3000)))
        .then((events: Array<SiteEventViewModel>) => this.onEventDataRetrieved(events))
        .catch((e) => {
            this.overviewDataLoadInProgress = false;
            this.overviewDataLoadFailed = true;
            this.overviewDataFailedErrorMessage = `Failed to load data with the following error. ${e}.`;
            console.error(e);
        });
    }
    
    onEventDataRetrieved(events: Array<SiteEventViewModel>): void {
        this.overviewDataLoadInProgress = false;
        let index = -1;
        events.forEach(x => {
            index++;
            x.Timestamp = new Date(x.Timestamp);
            x.EndTime = new Date(x.EndTime);
        });
        this.siteEvents = events;
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