<!-- src/components/modules/Overview/EventCalendarComponent.vue -->
<template>
    <div>
        <!-- DATEPICKER & TYPE SELECT-->
        <div v-if="calendarEvents.length > 0">
            <div xs12 sm3 class="mr-4">
                <text-field-component
                    v-model:value="calendarStart"
                    label="Date"
                    prepend-icon="event"
                    readonly
                    @click="datepickerModal = true"
                ></text-field-component>
                <dialog-component ref="dateDialog" v-model:value="datepickerModal"
                    full-width width="290px">
                    <date-picker-component 
                        v-model:value="calendarStart"
                        :allowed-dates="allowDatepickerDate">
                        <btn-component flat color="primary" @click="datepickerModal = false">Cancel</btn-component>
                        <btn-component flat color="primary" @click="$refs.dateDialog.save(calendarStart)">OK</btn-component>
                    </date-picker-component>
                </dialog-component>
            </div>
            <div xs12 sm3 class="text-xs-center">
                <select-component v-model:value="calendarType" :items="calendarTypeOptions" label="Type"></select-component>
            </div>
        </div>

        <!-- CALENDAR -->
        <calendar-component
            :value="calendarToday" 
            v-model:value="calendarStart"
            :weekdays="[1, 2, 3, 4, 5, 6, 0]"
            :type="calendarType"
            color="primary" ref="calendar">
            <!-- MONTH VIEW -->
            <template v-slot:day="{ date }">
                <div v-for="(event, eventIndex) in calendarEventsMap[date]"
                    :key="`month-item-${event.id}-${eventIndex}`"
                    @click="onEventClicked(event.data)"
                    class="calendar-event"
                    :class="getEventSeverityClass(event.data.Severity)">
                    {{event.title}}
                </div>
            </template>
            
            <!-- WITH-TIME VIEW -->
            <template v-slot:dayBody="{ date, timeToY, minutesToPixels }">
                <template  v-for="(event) in calendarEventsMap[date]">
                    <div
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
        </calendar-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import CalendarEvent from '@models/Common/CalendarEvent';
import { SiteEventViewModel } from '@generated/Models/Core/SiteEventViewModel';
import { SiteEventSeverity } from '@generated/Enums/Core/SiteEventSeverity';
import EventTimelineComponent from '@components/modules/Overview/EventTimelineComponent.vue';
import SiteEventDetailsComponent from '@components/modules/Overview/SiteEventDetailsComponent.vue';
import LinqUtils from '@util/LinqUtils';
import DateUtils from '@util/DateUtils';

@Options({
    components: {
        EventTimelineComponent,
        SiteEventDetailsComponent
    }
})
export default class EventCalendarComponent extends Vue {
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
    background-color: var(--color--primary-base);
    color: #ffffff;
    border: 1px solid var(--color--primary-base);
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
