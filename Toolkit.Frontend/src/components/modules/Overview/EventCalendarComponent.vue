<!-- src/components/modules/Overview/EventCalendarComponent.vue -->
<template>
    <div>
        <!-- CALENDAR -->
        <calendar-component
            :events="calendarEvents" @eventClicked="onEventClicked"
            :initialMode="initialMode"
            :allowedModes="allowedModes">
        </calendar-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { SiteEventViewModel } from '@generated/Models/Core/SiteEventViewModel';
import EventTimelineComponent from '@components/modules/Overview/EventTimelineComponent.vue';
import SiteEventDetailsComponent from '@components/modules/Overview/SiteEventDetailsComponent.vue';
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

    @Prop({ required: false, default: '' })
    initialMode: string;

    @Prop({ required: false, default: null })
    allowedModes: Array<string> | null;

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
                // allDay: x.Timestamp.getDate() !== x.EndTime.getDate(),
                classNames: ['calendar-event', `cevent-${x.Severity?.toLowerCase()}`]
                // color: '#FF0000'
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

    // getEventSeverityIcon(severity: SiteEventSeverity): string {
    //     if (severity == SiteEventSeverity.Information) {
    //         return 'info';
    //     } else if (severity == SiteEventSeverity.Warning) {
    //         return 'warning';
    //     } else if (severity == SiteEventSeverity.Error) {
    //         return 'error';
    //     } else if (severity == SiteEventSeverity.Fatal) {
    //         return 'report';
    //     } else {
    //         return '';
    //     }
    // }

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

<style lang="scss">
.calendar-event {
    cursor: pointer;
}
.fc-header-toolbar {
    display: flex;
    flex-wrap: wrap;
}
.fc-toolbar-chunk { margin-top: 5px; }
.fc-list-event-graphic { display: none; }
.fc-daygrid-event-dot { visibility: hidden; margin: 0; }
.fc-daygrid-dot-event .fc-event-title { font-weight: normal !important; }
.cevent-information {
    background-color: var(--color--info-base);
    border-color: var(--color--info-base);
    color: #fff !important;
    &:hover {
        background-color: var(--color--info-lighten1);
        td { background-color: var(--color--info-lighten1) !important; }
    }
}
.cevent-warning {
    background-color: var(--color--warning-base);
    border-color: var(--color--warning-base);
    color: #fff !important;
    &:hover {
        background-color: var(--color--warning-lighten1);
        td { background-color: var(--color--warning-lighten1) !important; }
    }
}
.cevent-error {
    background-color: var(--color--error-base);
    border-color: var(--color--error-base);
    color: #fff !important;
    &:hover {
        background-color: var(--color--error-lighten1);
        td { background-color: var(--color--error-lighten1) !important; }
    }
}
.cevent-fatal {
    background-color: #111;
    border-color: #111;
    color: #fff !important;
    &:hover {
        background-color: #222;
        td { background-color: #222 !important; }
    }
}
</style>
