<template>
    <div class="calendar-component mb-4" :class="rootClasses">
        <full-calendar :options="calendarOptions" ref="calendarElement" />
    </div>
</template>

<script lang="ts">
// https://fullcalendar.io/docs/vue
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
// import '@fullcalendar/core/vdom' // solves problem with Vite
import FullCalendar, { Calendar, CalendarOptions, DateRangeInput, DateSelectArg, EventApi, EventClickArg, EventInput } from '@fullcalendar/vue3';
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import listPlugin from '@fullcalendar/list';
import interactionPlugin from '@fullcalendar/interaction';
import { CalendarComponentEvent } from "./CalendarComponent.vue.models";

@Options({
    components: { FullCalendar }
})
export default class CalendarComponent extends Vue {
    @Prop({ required: true })
    events!: Array<CalendarComponentEvent<any>>;

    @Ref() readonly calendarElement!: Vue;

    localValue: string = "";

    calendarOptions: CalendarOptions = {
        plugins: [
            dayGridPlugin,
            timeGridPlugin,
            listPlugin,
            interactionPlugin // needed for dateClick
        ],
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,timeGridDay,listWeek'
        },
        initialView: 'dayGridMonth',
        eventTimeFormat: {
            hour: 'numeric',
            minute: 'numeric',
            // second: '2-digit',
            meridiem: false,
            hour12: false
        },
        // validRange: { start: '2022-05-01', end: '2022-07-01' },
        // initialEvents: this.internalCalendarEvents, // alternatively, use the `events` setting to fetch from a feed
        // events: this.internalCalendarEvents,
        editable: false,
        selectable: false,
        dayMaxEvents: true,
        weekends: true,
        firstDay: 1,
        allDaySlot: false,

        // select: this.handleDateSelect,
        eventClick: this.handleEventClick.bind(this),
        // eventsSet: this.handleEvents,
        views: {
            dayGridMonth: { },
            timeGrid: {
                slotLabelFormat: {
                    hour: 'numeric',
                    minute: '2-digit',
                    // second: '2-digit',
                    meridiem: false,
                    hour12: false
                }
            },
        },
        /* you can update a remote database when these fire:
        eventAdd:
        eventChange:
        eventRemove:
        */
    };
    currentEvents: EventApi[] = [];

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void {
        this.updateCalendarEvents();
        this.updateValidRange();
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get rootClasses(): any {
        return {

        };
    }

    get calendarApi(): Calendar {
        return (<any>this.calendarElement)?.getApi();
    }

    // https://fullcalendar.io/docs/event-parsing
    get internalCalendarEvents(): Array<EventInput> {
        return this.events;
    }

    ////////////////
    //  METHODS  //
    //////////////
    // handleDateSelect(selectInfo: DateSelectArg): void {
    // }
    updateValidRange(): void {
        const start = this.internalCalendarEvents.sort((a,b) => (<Date>a.start).getTime() - (<Date>b.start).getTime())[0];
        const end = this.internalCalendarEvents.sort((a,b) => (<Date>b.end).getTime() - (<Date>a.end).getTime())[0];
        let dateRange: DateRangeInput = {
            start: start?.start || null,
            end: end?.end || null
        }
        this.calendarOptions.validRange = dateRange;
    }

    handleEventClick(clickInfo: EventClickArg): void {
        const data = this.events.find(x => x.id == clickInfo.event.id).data;
        this.$emit("eventClicked", data);
    }

    handleEvents(events: EventApi[]): void {
      this.currentEvents = events
    }

    updateCalendarEvents(): void {
        // todo: optimize
        this.calendarApi.removeAllEvents();
        this.internalCalendarEvents.forEach(x => {
            this.calendarApi.addEvent(x);
        });
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
	
    /////////////////
    //  WATCHERS  //
    ///////////////
    @Watch("internalCalendarEvents")
    onEventsChanged(): void {
        this.updateValidRange();
        this.updateCalendarEvents();
    }
}
</script>

<style lang="scss">
.calendar-component {
    a {
        color: var(--color--text);
        &:hover {
            text-decoration: none;
        }
    }
}
</style>
