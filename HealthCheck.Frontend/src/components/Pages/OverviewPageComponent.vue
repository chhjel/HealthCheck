<!-- src/components/Pages/OverviewPageComponent.vue -->
<template>
    <div>
        <v-content class="pl-0">
        <v-container fluid fill-height class="content-root">
        <v-layout>
        <v-flex class="pl-4 pr-4 pb-4">
          <!-- CONTENT BEGIN -->
            
        <v-container grid-list-md>
            <h1 class="mb-4">Events overview</h1>

            <v-layout align-content-center wrap>
                <v-alert
                    :value="overviewDataLoadFailed"
                    type="error">
                {{ overviewDataFailedErrorMessage }}
                </v-alert>

                <v-progress-linear 
                    v-if="overviewDataLoadInProgress"
                    indeterminate color="green"></v-progress-linear>
                    
                <v-flex sm12 md7
                    v-if="!overviewDataLoadInProgress">
                    <v-alert :value="calendarEvents.length == 0" color="info" icon="sentiment_satisfied_alt" outline>
                        No events recorded the lately, things seems to be quiet.
                    </v-alert>
                    <event-calendar-component
                        :events="calendarEvents"
                        class="calendar" />
                </v-flex>
                
                <v-flex sm12 md5
                    v-if="!overviewDataLoadInProgress">
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
            x.Id = index.toString();
            x.Timestamp = new Date(x.Timestamp);
        });
        this.siteEvents = events;
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