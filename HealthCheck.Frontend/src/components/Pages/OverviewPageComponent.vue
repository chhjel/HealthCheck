<!-- src/components/Pages/OverviewPageComponent.vue -->
<template>
    <div>
        <v-content class="pl-0">
        <v-container fluid fill-height class="content-root">
        <v-layout>
        <v-flex class="pl-4 pr-4 pb-4">
          <!-- CONTENT BEGIN -->
            
        <v-container grid-list-md>
            <h1 class="mb-2">Events overview</h1>

            <v-layout align-content-center wrap>
                
                <v-flex sm12 md7 class="mb-4" v-if="showContent">
                    <v-alert :value="true" :color="summaryColor" :icon="summaryIcon">
                        <span v-if="summaryItems.length == 1">{{ summaryItems[0] }}</span>
                        <ul v-if="summaryItems.length > 1">
                            <li v-for="(summaryItem, index) in summaryItems"
                                :key="`summary-item-${index}`">
                                {{ summaryItem }}
                            </li>
                        </ul>
                    </v-alert>
                </v-flex>
            
                <v-alert
                    :value="overviewDataLoadFailed"
                    type="error">
                {{ overviewDataFailedErrorMessage }}
                </v-alert>

                <v-progress-linear
                    v-if="overviewDataLoadInProgress"
                    indeterminate color="green"></v-progress-linear>
                    
                <v-flex sm12 md7 v-if="showContent">
                    <event-calendar-component
                        :events="calendarEvents"
                        class="calendar" />
                </v-flex>
                
                <v-flex sm12 md5
                    v-if="showContent">
                    <event-timeline-component
                        :events="timelineEvents"
                        class="timeline" />
                </v-flex>
            </v-layout>
        </v-container>


          <!-- CONTENT END -->
        </v-flex>
        </v-layout>
        </v-container>
        </v-content>
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

@Component({
    components: {
        EventTimelineComponent,
        EventCalendarComponent
    }
})
export default class OverviewPageComponent extends Vue {
    @Prop({ required: true })
    options!: FrontEndOptionsViewModel;

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
        let thresholdDate = new Date();
        thresholdDate.setDate(thresholdDate.getDate() - 3);
        return this.siteEvents.filter(x => x.Timestamp >= thresholdDate);
    }

    get showContent(): boolean {
        return !this.overviewDataLoadInProgress && !this.overviewDataLoadFailed;
    }

    get summaryItems(): Array<string> {
        let relevantEvents = this.getSummaryEvents();
        if (relevantEvents.length == 0) {
            return ["All Systems Operational."];
        }
        
        let severity = this.getSummarySeverity();
        let relevantMessages = relevantEvents
            .filter(x => x.Severity == severity)
            .map(x => x.Title);
        return relevantMessages;
    }

    get summaryColor(): string {
        let severity = this.getSummarySeverity();
        if (severity == null) {
            return "success";
        }
        else if (severity == SiteEventSeverity.Information) {
            return "info";
        }
        else if (severity == SiteEventSeverity.Warning) {
            return "warning";
        }
        else if (severity == SiteEventSeverity.Error || severity == SiteEventSeverity.Fatal) {
            return "error";
        }
        else {
            return "info";
        }
    }

    get summaryIcon(): string {
        let severity = this.getSummarySeverity();
        if (severity == null) {
            return "sentiment_satisfied_alt";
        }
        else if (severity == SiteEventSeverity.Information) {
            return "info";
        }
        else if (severity == SiteEventSeverity.Warning) {
            return "warning";
        }
        else if (severity == SiteEventSeverity.Error || severity == SiteEventSeverity.Fatal) {
            return "error";
        }
        else {
            return "sentiment_satisfied_alt";
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
        });
        this.siteEvents = events;
    }

    getSummaryEvents(): Array<SiteEventViewModel> {
        let thresholdDate = new Date();
        thresholdDate.setHours(thresholdDate.getHours() - 1);
        return this.siteEvents.filter(x => x.Timestamp >= thresholdDate);
    }

    getSummarySeverity(): SiteEventSeverity | null {
        let relevantEvents = this.getSummaryEvents();
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
}
</script>

<style scoped>
.content-root {
    /* margin: auto;
    max-width: 800px; */
}
</style>

<style>
</style>