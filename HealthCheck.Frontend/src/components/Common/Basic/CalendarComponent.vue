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
import FullCalendar, { Calendar, CalendarOptions, DateSelectArg, EventApi, EventClickArg, EventInput } from '@fullcalendar/vue3';
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
        views: { dayGridMonth: { } },
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
        this.internalCalendarEvents.forEach(x => {
            this.calendarApi.addEvent(x);
        });
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

    handleEventClick(clickInfo: EventClickArg): void {
        const data = this.events.find(x => x.id == clickInfo.event.id).data;
        this.$emit("eventClicked", data);
    }

    handleEvents(events: EventApi[]): void {
      this.currentEvents = events
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
	
    /////////////////
    //  WATCHERS  //
    ///////////////
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
