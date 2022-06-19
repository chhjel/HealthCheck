<!-- src/components/modules/Overview/EventCalendarComponent.vue -->
<template>
    <div>
        <!-- CALENDAR -->
        <calendar-component :events="calendarEvents" @eventClicked="onEventClicked">
        </calendar-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import { SiteEventViewModel } from '@generated/Models/Core/SiteEventViewModel';
import { SiteEventSeverity } from '@generated/Enums/Core/SiteEventSeverity';
import EventTimelineComponent from '@components/modules/Overview/EventTimelineComponent.vue';
import SiteEventDetailsComponent from '@components/modules/Overview/SiteEventDetailsComponent.vue';
import LinqUtils from '@util/LinqUtils';
import DateUtils from '@util/DateUtils';
import { CalendarComponentEvent } from "@components/Common/Basic/CalendarComponent.vue.models";

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

    ////////////////
    //  GETTERS  //
    //////////////
    get calendarEvents(): Array<CalendarComponentEvent<SiteEventViewModel>>
    {
        let mappedEvents: Array<CalendarComponentEvent<SiteEventViewModel>> = this.events.map(x =>
        {
            return {
                id: x.Id,
                title: x.Title,
                start: x.Timestamp,
                end: this.getEventEndTime(x),
                data: x,
                allDay: x.Timestamp.getDate() !== x.EndTime.getDate(),
                color: '#FF0000',
            };
        });
        
        return mappedEvents;
    }

    ////////////////
    //  METHODS  //
    //////////////
    getEventEndTime(event: SiteEventViewModel) : Date | null {
        if (event.Duration > 1) {
            let date = new Date(event.Timestamp.getTime() + event.Duration * 60000);
            return date;
        } else {
            return null;
        }
    }
    getTimePartOnlyInCalendarFormat(date: Date | null) : string | null {
        if (date == null) return null;
        return date.getHours() + ":" + date.getMinutes();
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
</style>
