<!-- src/components/modules/Overview/EventCalendarComponent.vue -->
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
                <div v-for="(event, eventIndex) in calendarEventsMap[date]"
                    :key="`month-item-${event.id}-${eventIndex}`"
                    v-ripple
                    @click="onEventClicked(event.data)"
                    class="calendar-event"
                    :class="getEventSeverityClass(event.data.Severity)">
                    {{event.title}}
                </div>
            </template>
            
            <!-- WITH-TIME VIEW -->
            <template v-slot:dayBody="{ date, timeToY, minutesToPixels }">
                <template  v-for="(event) in calendarEventsMap[date]">
                    <div v-ripple
                        v-if="event.time"
                        :key="`day-item-${event.id}`"
                        :style="{
                            top: timeToY(event.time) + 'px',
                            height: minutesToPixels(event.duration) + 'px',
                            'min-height': '20px',
                            width: getEventWithTimeWidth(event.data),
                            left: getEventWithTimeLeft(event.data)
                        }"
                        class="calendar-event with-time"
                        :class="getEventSeverityClass(event.data.Severity)"
                        v-html="event.title"
                        @click="onEventClicked(event.data)"
                    ></div>
                </template>
            </template>
        </v-calendar>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import FrontEndOptionsViewModel from  '../../../models/Common/FrontEndOptionsViewModel';
import CalendarEvent from  '../../../models/Common/CalendarEvent';
import SiteEventViewModel from  '../../../models/modules/SiteEvents/SiteEventViewModel';
import { SiteEventSeverity } from  '../../../models/modules/SiteEvents/SiteEventSeverity';
import EventTimelineComponent from '../Overview/EventTimelineComponent.vue';
import SiteEventDetailsComponent from '../Overview/SiteEventDetailsComponent.vue';
import LinqUtils from  '../../../util/LinqUtils';
import DateUtils from  '../../../util/DateUtils';

@Component({
    components: {
        EventTimelineComponent,
        SiteEventDetailsComponent
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
        // Duplicate events that go across several days
        let mappedEvents = this.events.map(x =>
        {
            return {
                id: x.Id,
                title: x.Title,
                details: x.Description,
                date: this.getCalendarDateFormat(x.Timestamp),
                time: this.getCalendarTimeFormat(x.Timestamp),
                endTime: this.getEventEndTime(x),
                open: false,
                data: x,
                duration: x.Duration,
                dateTime: x.Timestamp
            };
        });
        
        this.events
            .filter(x =>  x.Timestamp.getDate() !== x.EndTime.getDate())
            .forEach(x => {
                let fromDate = new Date(x.Timestamp);
                fromDate.setHours(x.Timestamp.getHours() + 24);
                let fromDay = fromDate.getDate();
                let toDay = x.EndTime.getDate();
                
                let toDate = new Date(x.EndTime);
                toDate.setHours(toDate.getHours() + 24);
                let now = new Date();
                let secondLastDay = new Date(x.EndTime);
                secondLastDay.setDate(x.EndTime.getDate() - 1)

                for (let d = new Date(fromDate); d <= toDate; d.setDate(d.getDate() + 1)) {
                    if (DateUtils.IsDatePastDay(d, x.EndTime)) {
                        break;
                    }

                    let date = new Date(d);
                    let timestamp = new Date(x.Timestamp);
                    let duration = x.Duration;
                    let endTime = this.getEventEndTime(x);

                    let isLastDay = DateUtils.IsDatePastDay(d, secondLastDay);
                    if (isLastDay === true)
                    {
                        duration = (x.EndTime.getHours() * 60) + x.EndTime.getMinutes();
                        timestamp.setHours(0);
                        timestamp.setMinutes(0);
                        timestamp.setMilliseconds(0);
                    }
                    else
                    {
                        duration = 24 * 60;
                        timestamp.setHours(0);
                        timestamp.setMinutes(0);
                        timestamp.setMilliseconds(0);
                    }

                    mappedEvents.push({
                        id: x.Id,
                        title: x.Title,
                        details: x.Description,
                        date: this.getCalendarDateFormat(date),
                        time: this.getCalendarTimeFormat(timestamp),
                        endTime: endTime,
                        open: false,
                        data: x,
                        duration: duration,
                        dateTime: timestamp
                    });
                }
            });

        return mappedEvents;
    }

    get calendarEventsMap(): any {
        const map: any = {}
        this.calendarEvents
            .sort((a, b) => a.dateTime.getTime() - b.dateTime.getTime())
            .forEach(e => (map[e.date] = map[e.date] || []).push(e))
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
        return this.calendarEvents
            .some(x => {
                let start = x.data.Timestamp;
                let end = x.data.EndTime;
                return DateUtils.DatesAreOnSameDay(date, start)
                    || DateUtils.DatesAreOnSameDay(date, end)
                    || date.getTime() > start.getTime() && date.getTime() < end.getTime();
            });
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

    getEventEndTime(event: SiteEventViewModel) : string | null {
        if (event.Duration > 1) {
            let date = new Date(event.Timestamp.getTime() + event.Duration * 60000);
            return this.getCalendarTimeFormat(date);
        } else {
            return null;
        }
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

    getEventWithTimeWidth(event: SiteEventViewModel): string
    {
        let numberOfCollidingEvents = this.events
            .filter(x => DateUtils.DateRangeOverlaps(x.Timestamp, x.EndTime, event.Timestamp, event.EndTime))
            .length;
        return `${Math.floor(100 / numberOfCollidingEvents)}%`;
    }

    getEventWithTimeLeft(event: SiteEventViewModel): string
    {
        let numberOfCollidingEvents = this.events
            .filter(x => DateUtils.DateRangeOverlaps(x.Timestamp, x.EndTime, event.Timestamp, event.EndTime))
            .length;
        let numberOfCollidingEventsBeforeIt = 0;
        for(let i=0; i<this.events.length; i++)
        {
            let ev = this.events[i];
            if (ev === event) {
                break;
            }

            if (DateUtils.DateRangeOverlaps(ev.Timestamp, ev.EndTime, event.Timestamp, event.EndTime))
            {
                numberOfCollidingEventsBeforeIt++;
            }
        }

        let width = Math.floor(100 / numberOfCollidingEvents);
        return `${width * numberOfCollidingEventsBeforeIt}%`;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onEventClicked(event: SiteEventViewModel): void {
        this.$emit("eventClicked", event);
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
    cursor: default;
}
.v-calendar-weekly__day-label:hover {
    text-decoration: none;
}
.v-calendar-weekly__day-month {
    font-size: 18px;
    color: #9c8888;
}
/* .v-calendar-weekly__day.v-past > .v-calendar-weekly__day-label:only-child::after,
.v-calendar-weekly__day.v-past > .v-calendar-weekly__day-label+.v-calendar-weekly__day-month::after {
    content: 'done';
    color: var(--v-success-lighten5);

    font-family: 'Material Icons';
    font-weight: normal;
    font-style: normal;
    font-size: 51px;
    line-height: 1;
    letter-spacing: normal;
    text-transform: none;
    display: block;
    white-space: nowrap;
    word-wrap: normal;
    direction: ltr;
    -webkit-font-feature-settings: 'liga';
    -webkit-font-smoothing: antialiased;
    
    font-size: 5vw;
}
.v-calendar-weekly__day.v-past > .v-calendar-weekly__day-label+.v-calendar-weekly__day-month::after {
    margin-left: -37px;
}
@media (min-width: 800px) {
    .v-calendar-weekly__day.v-past > .v-calendar-weekly__day-label:only-child::after,
    .v-calendar-weekly__day.v-past > .v-calendar-weekly__day-label+.v-calendar-weekly__day-month::after {
        font-size: 50px;
    }
} */
</style>