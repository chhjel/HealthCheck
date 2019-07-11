<!-- src/components/Overview/EventCalendarComponent.vue -->
<template>
    <div>
        <!-- DATEPICKER & TYPE SELECT-->
        <v-layout v-if="calendarEvents.length > 0">
            <v-flex xs12 sm3 class="mr-4">
                <v-dialog ref="dateDialog" v-model="datepickerModal"
                    :return-value.sync="calendarStart" lazy
                    full-width width="290px">
                    <template v-slot:activator="{ on }">
                    <v-text-field
                        v-model="calendarStart"
                        label="Date"
                        prepend-icon="event"
                        readonly
                        v-on="on"
                    ></v-text-field>
                    </template>
                    <v-date-picker 
                        v-model="calendarStart"
                        :allowed-dates="allowDatepickerDate"
                        scrollable>
                    <v-spacer></v-spacer>
                    <v-btn flat color="primary" @click="datepickerModal = false">Cancel</v-btn>
                    <v-btn flat color="primary" @click="$refs.dateDialog.save(calendarStart)">OK</v-btn>
                    </v-date-picker>
                </v-dialog>
            </v-flex>
            <v-flex xs12 sm3 class="text-xs-center">
                <v-select v-model="calendarType" :items="calendarTypeOptions" label="Type"></v-select>
            </v-flex>
        </v-layout>

        <!-- CALENDAR -->
        <v-calendar
            :value="calendarToday" 
            v-model="calendarStart"
            :weekdays="[1, 2, 3, 4, 5, 6, 0]"
            :type="calendarType"
            color="primary" ref="calendar">
            <!-- MONTH VIEW -->
            <template v-slot:day="{ date }">
                <template v-for="(event, eventIndex) in calendarEventsMap[date]">
                    <v-menu
                        :key="`month-item-${event.id}-${eventIndex}`"
                        v-model="event.open" full-width offset-x>
                        <template v-slot:activator="{ on }">
                            <div v-ripple v-on="on"
                                class="calendar-event"
                                :class="getEventSeverityClass(event.data.Severity)">
                                {{event.title}}
                            </div>
                        </template>
                        <v-card color="grey lighten-4" flat
                            min-width="350px" max-width="800px">
                            <v-toolbar dark
                                :color="getEventSeverityColor(event.data.Severity)">
                                <v-icon v-text="getEventSeverityIcon(event.data.Severity)"/>
                                <!-- <v-btn icon>
                                    <v-icon>edit</v-icon>
                                </v-btn> -->
                                <v-toolbar-title>{{ event.title }}</v-toolbar-title>
                                <v-spacer></v-spacer>
                                <v-toolbar-title class="subheading">@ {{ event.date }} {{ event.time }}</v-toolbar-title>
                                <!-- <v-btn icon>
                                    <v-icon>favorite</v-icon>
                                </v-btn> -->
                                <!-- <v-btn icon>
                                    <v-icon>more_vert</v-icon>
                                </v-btn> -->
                            </v-toolbar>
                            <v-card-title primary-title>
                                <span>
                                    {{ event.data.Description }}

                                    <div v-if="event.data.RelatedLinks.length > 0" class="mt-4">
                                        <h4>Related links</h4>
                                        <ul>
                                            <li v-for="(link, linkIndex) in event.data.RelatedLinks"
                                                :key="`month-item-${event.id}-link-${linkIndex}`">
                                                <a :href="link.Url">{{link.Text}}</a>
                                            </li>
                                        </ul>
                                    </div>

                                    <div class="caption mt-4">
                                        This is a {{ event.data.Severity.toString().toLowerCase() }}-level event.<br />
                                        EventTypeId: {{ event.data.EventTypeId }}
                                    </div>
                                </span>
                            </v-card-title>
                            <v-card-actions>
                                <v-btn flat color="secondary">
                                    Close
                                </v-btn>
                            </v-card-actions>
                        </v-card>
                    </v-menu>
                </template>
            </template>
            
            <!-- WITH-TIME VIEW -->
            <template v-slot:dayBody="{ date, timeToY, minutesToPixels }">
                <template v-for="event in calendarEventsMap[date]">
                    <div
                        v-if="event.time"
                        :key="`day-item-${event.id}`"
                        :style="{ top: timeToY(event.time) + 'px', height: minutesToPixels(event.duration) + 'px' }"
                        class="calendar-event with-time"
                        :class="getEventSeverityClass(event.data.Severity)"
                        v-html="event.title"
                    ></div>
                </template>
            </template>
        </v-calendar>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import FrontEndOptionsViewModel from '../../models/Page/FrontEndOptionsViewModel';
import CalendarEvent from '../../models/Common/CalendarEvent';
import SiteEventViewModel from '../../models/SiteEvents/SiteEventViewModel';
import { SiteEventSeverity } from '../../models/SiteEvents/SiteEventSeverity';
import EventTimelineComponent from '../Overview/EventTimelineComponent.vue';

@Component({
    components: {
        EventTimelineComponent
    }
})
export default class OverviewPageComponent extends Vue {
    @Prop({ required: true })
    events!: Array<SiteEventViewModel>;

    datepickerModal: boolean = false;
    calendarStart: string = this.nowDateString;
    calendarToday: string = this.nowTimeString;
    calendarType: string = 'month';
    calendarTypeOptions: Array<any> = [
        { text: 'Day', value: 'day' },
        { text: '4 Day', value: '4day' },
        { text: 'Week', value: 'week' },
        { text: 'Month', value: 'month' }
      ];

    ////////////////
    //  GETTERS  //
    //////////////
    get calendarEvents(): Array<CalendarEvent<SiteEventViewModel>>
    {
        return this.events.map(x =>
        {
            return {
                id: x.Id,
                title: x.Title,
                details: x.Description,
                date: this.getCalendarDateFormat(x.Timestamp),
                time: this.getCalendarTimeFormat(x.Timestamp),
                open: false,
                data: x,
                duration: 45
            };
        });
    }

    get calendarEventsMap(): any {
        const map: any = {}
        this.calendarEvents.forEach(e => (map[e.date] = map[e.date] || []).push(e))
        return map
    }

    get nowDateString(): string {
        return this.getCalendarDateFormat(new Date());
    }

    get nowTimeString(): string {
        return this.getCalendarDateTimeFormat(new Date());
    }

    ////////////////
    //  METHODS  //
    //////////////
    allowDatepickerDate(dateStr: string): boolean {
        let date = new Date(dateStr);
        return this.calendarEvents.map(x => x.data.Timestamp).some(x => 
            x.getMonth() == date.getMonth()
            && x.getDate() == date.getDate()
            && x.getFullYear() == date.getFullYear());
    }

    getCalendarDateTimeFormat(date: Date): string {
        //@ts-ignore
        return `${date.getFullYear()}-${(date.getMonth() + 1).toString().padZero(2)}-${date.getDate().toString().padZero(2)} ${date.getHours().toString().padZero(2)}:${date.getMinutes().toString().padZero(2)}:${date.getSeconds().toString().padZero(2)}`;
    }
    getCalendarTimeFormat(date: Date): string {
        //@ts-ignore
        return `${(date.getHours().toString().padZero(2))}:${date.getMinutes().toString().padZero(2)}`;
    }
    getCalendarDateFormat(date: Date): string {
        //@ts-ignore
        return `${date.getFullYear()}-${(date.getMonth() + 1).toString().padZero(2)}-${date.getDate().toString().padZero(2)}`;
    }

    getEventSeverityClass(severity: SiteEventSeverity): string {
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

    getEventSeverityColor(severity: SiteEventSeverity): string {
        if (severity == SiteEventSeverity.Information) {
            return 'info';
        } else if (severity == SiteEventSeverity.Warning) {
            return 'warning';
        } else if (severity == SiteEventSeverity.Error) {
            return 'error';
        } else if (severity == SiteEventSeverity.Fatal) {
            return 'black';
        } else {
            return '';
        }
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
.calendar-event.with-time {
    position: absolute;
    right: 4px;
    margin-right: 0px;
}
.fatal {
    background-color: #000;
}
</style>

<style>
.v-calendar-weekly__day {
    min-height: 100px;
}
.v-calendar-weekly__day-label {
    font-size: 18px;
    color: #9c8888;
}
.v-calendar-weekly__day-month {
    font-size: 18px;
    color: #9c8888;
}
</style>