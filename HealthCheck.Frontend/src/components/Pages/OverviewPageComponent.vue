<!-- src/components/Pages/OverviewPageComponent.vue -->
<template>
    <div>
        <v-content class="pl-0">
         <v-container fluid fill-height class="content-root">
          <v-layout>
          <v-flex>
            <!-- CONTENT BEGIN -->
            <v-calendar
                v-show="false"
                :now="today" :value="today" 
                v-model="start"
                :type="type"
                color="primary" ref="calendar">
            <template v-slot:day="{ date }">
                <template v-for="event in eventsMap[date]">
                    <v-menu :key="event.title" v-model="event.open" full-width offset-x>
                        <template v-slot:activator="{ on }">
                            <div v-if="!event.time" v-ripple class="calendar-event" v-on="on" v-html="event.title"></div>
                        </template>
                        <v-card color="grey lighten-4" min-width="350px" flat>
                            <v-toolbar color="primary" dark>
                                <v-btn icon>
                                    <v-icon>edit</v-icon>
                                </v-btn>
                                <v-toolbar-title v-html="event.title"></v-toolbar-title>
                                <v-spacer></v-spacer>
                                <v-btn icon>
                                    <v-icon>favorite</v-icon>
                                </v-btn>
                                <v-btn icon>
                                    <v-icon>more_vert</v-icon>
                                </v-btn>
                            </v-toolbar>
                            <v-card-title primary-title>
                                <span v-html="event.details"></span>
                            </v-card-title>
                            <v-card-actions>
                                <v-btn flat color="secondary">
                                    Cancel
                                </v-btn>
                            </v-card-actions>
                        </v-card>
                    </v-menu>
                </template>
            </template>
            </v-calendar>

            <!-- PREV / NEXT -->
            <v-flex v-show="false">
                <v-flex 
                    sm4 xs12 class="text-sm-left text-xs-center">
                    <v-btn @click="$refs.calendar.prev()">
                        <v-icon dark left>
                            keyboard_arrow_left
                        </v-icon>
                        Prev
                    </v-btn>
                </v-flex>
                <v-flex sm4 xs12 class="text-xs-center">
                    <v-select v-model="type" :items="typeOptions" label="Type"></v-select>
                </v-flex>
                <v-flex sm4 xs12 class="text-sm-right text-xs-center">
                    <v-btn @click="$refs.calendar.next()">
                        Next
                        <v-icon right dark>
                            keyboard_arrow_right
                        </v-icon>
                    </v-btn>
                </v-flex>
            </v-flex>

            <!-- LAST DAYS TIMELINE -->
            <div class="timeline">
                <h2>Events last 3 days</h2>
                <v-timeline align-top dense>
                    <v-timeline-item small hide-dot class="pb-0">
                        <span>TODAY</span>
                    </v-timeline-item>

                    <v-timeline-item color="error" small>
                        <v-layout pt-3>
                            <v-flex xs3>
                                <strong>5pm</strong>
                            </v-flex>
                            <v-flex>
                                <strong>Faults in integration with Finn.no</strong>
                                <div class="caption">Some short details here.</div>
                            </v-flex>
                        </v-layout>
                    </v-timeline-item>

                    <v-timeline-item color="warning" small>
                        <v-layout wrap pt-3>
                            <v-flex xs3>
                                <strong>3-4pm</strong>
                            </v-flex>
                            <v-flex>
                                <strong>Page slowness detected</strong>
                                <div class="caption">Page load latency was increased by 222%.</div>
                            </v-flex>
                        </v-layout>
                    </v-timeline-item>
                    
                    <v-timeline-item small hide-dot class="pb-0">
                        <span>Yesterday</span>
                    </v-timeline-item>
                    <v-timeline-item color="error" small>
                        <v-layout pt-3>
                            <v-flex xs3>
                                <strong>12pm</strong>
                            </v-flex>
                            <v-flex>
                                <strong>Site unresponsive for 2 minutes</strong>
                                <div class="caption">Site down for short time.</div>
                            </v-flex>
                        </v-layout>
                    </v-timeline-item>

                    <v-timeline-item color="warning" small>
                        <v-layout pt-3>
                            <v-flex xs3>
                                <strong>9-11am</strong>
                            </v-flex>
                            <v-flex>
                                <strong>New version deployed</strong>
                                <div class="caption">Site down for short time.</div>
                            </v-flex>
                        </v-layout>
                    </v-timeline-item>
                    
                    <v-timeline-item small hide-dot class="pb-0">
                        <span>Saturday, 6. July</span>
                    </v-timeline-item>
                    <v-timeline-item color="success" small>
                        <v-layout pt-3>
                            <v-flex xs3>
                                <strong>No events</strong>
                            </v-flex>
                            <v-flex>
                            </v-flex>
                        </v-layout>
                    </v-timeline-item>
                </v-timeline>
            </div>

          <!-- CONTENT END -->
          </v-flex>
          </v-layout>
         </v-container>
        </v-content>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import FrontEndOptionsViewModel from '../../models/FrontEndOptionsViewModel';

@Component({
    components: {
    }
})
export default class OverviewPageComponent extends Vue {
    @Prop({ required: true })
    options!: FrontEndOptionsViewModel;

    start: string = '2019-01-01';
    today: string = '2019-01-08';
    events: Array<any> = [
        {
          title: 'Vacation',
          details: 'Going to the beach!',
          date: '2018-12-30',
          time: '12:00',
          open: false
        },
        {
          title: 'Vacation',
          details: 'Going to the beach!',
          date: '2018-12-31',
          open: false
        },
        {
          title: 'Vacation',
          details: 'Going to the beach!',
          date: '2019-01-01',
          open: false
        },
        {
          title: 'Meeting',
          details: 'Spending time on how we do not have enough time',
          date: '2019-01-07',
          open: false
        },
        {
          title: '30th Birthday',
          details: 'Celebrate responsibly',
          date: '2019-01-03',
          open: false
        },
        {
          title: 'New Year',
          details: 'Eat chocolate until you pass out',
          date: '2019-01-01',
          open: false
        },
        {
          title: 'Conference',
          details: 'Mute myself the whole time and wonder why I am on this call',
          date: '2019-01-21',
          open: false
        },
        {
          title: 'Hackathon',
          details: 'Code like there is no tommorrow',
          date: '2019-02-01',
          open: false
        }
      ];
    type: string = 'month';
    typeOptions: Array<any> = [
        { text: 'Day', value: 'day' },
        { text: '4 Day', value: '4day' },
        { text: 'Week', value: 'week' },
        { text: 'Month', value: 'month' }
      ];

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get eventsMap () {
        const map: any = {}
        this.events.forEach(e => (map[e.date] = map[e.date] || []).push(e))
        return map
    }

    ////////////////
    //  METHODS  //
    //////////////
    open(event: any): void {
        alert(event.title)
    }
}
</script>

<style scoped>
.calendar-event {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    border-radius: 2px;
    background-color: var(--v-primary-base);
    color: #ffffff;
    border: 1px solid var(--v-primary-base);
    width: 100%;
    font-size: 12px;
    padding: 3px;
    cursor: pointer;
    margin-bottom: 1px;
}
.timeline {
    margin: auto;
    max-width: 600px;
}
</style>

<style>
</style>