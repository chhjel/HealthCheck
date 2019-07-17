<!-- src/components/Overview/EventTimelineComponent.vue -->
<template>
    <div class="root">
        <v-timeline align-top dense>
            <v-timeline-item color="info" small v-if="timelineEventGroups.length == 0">
                <v-layout pt-3>
                    <div class="mr-4">
                        <strong>No events</strong>
                    </div>
                </v-layout>
            </v-timeline-item>
            <template v-for="group in timelineEventGroups">
                <v-timeline-item 
                    :key="`timeline-group-${group.index}-header`"
                    small hide-dot class="pb-0">
                    <span>{{group.title}}</span>
                </v-timeline-item>

                <v-timeline-item
                    v-for="event in group.events"
                    :key="`timeline-group-${group.index}-item-${event.Id}`"
                    :color="getTimelineItemColor(event)"
                    small>
                    <v-layout pt-3 class="timeline-item" @click="onEventClicked(event)">
                        <div class="mr-4 pt-1 timeline-item-time">
                            <strong>{{getTimelineItemTimeString(event)}}</strong>
                        </div>
                        <v-flex>
                            <strong class="timeline-item-title">{{ event.Title }}</strong>
                            <div class="caption timeline-item-description">{{ event.Description }}</div>
                        </v-flex>
                    </v-layout>
                </v-timeline-item>
            </template>
        </v-timeline>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import SiteEventViewModel from '../../models/SiteEvents/SiteEventViewModel';
import LinqUtils from "../../util/LinqUtils";
import { SiteEventSeverity } from "../../models/SiteEvents/SiteEventSeverity";

interface TimelineGroup {
    index: number;
    title: string;
    events: Array<SiteEventViewModel>;
}

@Component({
    components: {}
})
export default class EventTimelineComponent extends Vue {
    @Prop({ required: true })
    events!: Array<SiteEventViewModel>;
    
    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get timelineEventGroups(): Array<TimelineGroup> {
        let index = -1;
        let sortedEvents = this.events.sort((a, b) => a.Timestamp > b.Timestamp ? -1 : a < b ? 1 : 0);
        return LinqUtils.GroupByInto(sortedEvents, (item) => this.createTimelineGroupName(item.Timestamp), (title, items) => {
            return {
                index: index++,
                title: title,
                events: items.sort((a, b) => a.Timestamp > b.Timestamp ? -1 : a < b ? 1 : 0)
            };
        });
    }

    ////////////////
    //  METHODS  //
    //////////////
    getTimelineItemTimeString(event: SiteEventViewModel): string {
        if (event.Duration > 1) {
            // @ts-ignore
            let from = `${event.Timestamp.getHours().toString().padZero(2)}:${event.Timestamp.getMinutes().toString().padZero(2)}`;
            // @ts-ignore
            let toDate = new Date(event.Timestamp.getTime());
            toDate.setMinutes(event.Timestamp.getMinutes() + event.Duration);
            let to = `${toDate.getHours().toString().padZero(2)}:${toDate.getMinutes().toString().padZero(2)}`;
            return `${from} - ${to}`;
        } else {
            // @ts-ignore
            return `${event.Timestamp.getHours().toString().padZero(2)}:${event.Timestamp.getMinutes().toString().padZero(2)}`;
        }
    }

    getTimelineItemColor(event: SiteEventViewModel): string {
        if (event.Severity == SiteEventSeverity.Warning) return "warning";
        else if (event.Severity == SiteEventSeverity.Error) return "error";
        else if (event.Severity == SiteEventSeverity.Fatal) return "black";
        else return "info";
    }

    dayNames: string[] = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    createTimelineGroupName(date: Date): string {
        if (this.isToday(date)) return "Today";
        else if (this.isToday(date, -1)) return "Yesterday";
        
        let locale = 'en-US';
        let weekday = date.toLocaleDateString(locale, { weekday: 'long' });
        let month = date.toLocaleDateString(locale, { month: 'long' });
        return `${weekday}, ${date.getDate()}. ${month}`;
    }

    isToday(date: Date, daysOffset: number = 0): boolean {
        const today = new Date()
        return date.getDate() == (today.getDate() + daysOffset) &&
            date.getMonth() == today.getMonth() &&
            date.getFullYear() == today.getFullYear()
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
.timeline-item {
    cursor: pointer;
}
.timeline-item:hover {
    background-color: #f1eded;
}
.timeline-item-time {
    font-size: 18px;
}
.timeline-item-title {
    font-size: 18px;
}
.timeline-item-description {
    font-size: 16px !important;
}
</style>