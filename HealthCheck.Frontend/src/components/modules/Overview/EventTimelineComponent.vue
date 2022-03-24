<!-- src/components/modules/Overview/EventTimelineComponent.vue -->
<template>
    <div class="root event-timeline-component">
        <v-timeline align-top dense>
            <v-timeline-item color="info" small v-if="timelineEventGroups.length == 0">
                <v-layout pt-3>
                    <div class="mr-4">
                        <strong>No recent events</strong>
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
                            <strong>{{getTimelineItemTimeString(event, group)}}</strong>
                        </div>
                        <v-flex>
                            <strong class="timeline-item-title">
                                {{ event.Title }}
                                <strong class="timeline-item-title-resolved-label" v-if="event.Resolved">
                                    Resolved
                                </strong>
                            </strong>
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
import SiteEventViewModel from  '../../../models/modules/SiteEvents/SiteEventViewModel';
import LinqUtils from  '../../../util/LinqUtils';
import { SiteEventSeverity } from  '../../../models/modules/SiteEvents/SiteEventSeverity';
import DateUtils from  '../../../util/DateUtils';

interface TimelineGroup {
    index: number;
    title: string;
    date: Date;
    events: Array<SiteEventViewModel>;
}

@Component({
    components: {}
})
export default class EventTimelineComponent extends Vue {
    @Prop({ required: true })
    events!: Array<SiteEventViewModel>;

    @Prop()
    fromDate!: Date | null;
    
    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get thresholdFromDate(): Date
    {
        if (this.fromDate != null) {
            return this.fromDate;
        } else {
            let date = new Date();
            date.setDate(date.getDate() - 3);
            date.setHours(23);
            date.setMinutes(59);
            return date;
        }
    }

    get timelineEventGroups(): Array<TimelineGroup> {
        let index = -1;
        let sortedEvents = this.events;
        this.duplicateNeededEventsAcrossDays(this.events).forEach(x => sortedEvents.push(x));
        sortedEvents = sortedEvents
            .sort((a, b) => a.Timestamp > b.Timestamp ? -1 : a < b ? 1 : 0)
            .filter(x => x.Timestamp.getTime() >= this.thresholdFromDate.getTime());

        return LinqUtils.GroupByInto(sortedEvents,
            (item) => this.createTimelineGroupName(item.Timestamp),
            (title, items) => {
                let groupEvents = items.sort((a, b) => a.Timestamp.getTime() > b.Timestamp.getTime() ? -1 : 0);

                return {
                    index: index++,
                    title: title,
                    date: items[0].Timestamp,
                    events: groupEvents
                };
        });
    }

    ////////////////
    //  METHODS  //
    //////////////
    duplicateNeededEventsAcrossDays(items: Array<SiteEventViewModel>): Array<SiteEventViewModel> {
        let dupes = new Array<SiteEventViewModel>();
        items.filter(item => item.Timestamp.getDate() != item.EndTime.getDate())
            .forEach(x => {
                let fromDate = new Date(x.Timestamp);
                fromDate.setHours(x.Timestamp.getHours() + 24);
                
                let toDate = new Date(x.EndTime);
                toDate.setHours(toDate.getHours() + 24);
                for (let d = new Date(fromDate); d <= toDate; d.setDate(d.getDate() + 1)) {
                    if (DateUtils.IsDatePastDay(d, x.EndTime)) {
                        break;
                    }

                    let dupe = Object.assign({}, x);
                    (<any>dupe).ActualStart = x.Timestamp;
                    
                    let date = new Date(d);
                    date.setHours(0);
                    date.setMinutes(0);
                    date.setSeconds(0);
                    date.setMilliseconds(0);
                    dupe.Timestamp = date;

                    dupes.push(dupe);
                }
            });
        return dupes;
    }

    getTimelineItemTimeString(event: SiteEventViewModel, group: TimelineGroup): string {
        let timeFormat = 'HH:mm';
        let dateFormat = 'dd. MMM';

        let timestamp = <Date>(<any>event).ActualStart || event.Timestamp;
        
        // Across days
        if (timestamp.getDate() != event.EndTime.getDate()) {
            let date = DateUtils.FormatDate(timestamp, dateFormat);
            let from = DateUtils.FormatDate(timestamp, timeFormat);
            let to = DateUtils.FormatDate(event.EndTime, timeFormat);

            if (group.date.getDate() == timestamp.getDate()) {
                return `${from} →`;
            }
            else if (group.date.getDate() == event.EndTime.getDate()) {
                return `→ ${to}`;
            }
            else {
                return `All day`;
            }
        }
        // More than an instant
        else if (event.Duration > 1) {
            let from = DateUtils.FormatDate(event.Timestamp, timeFormat);
            let to = DateUtils.FormatDate(event.EndTime, timeFormat);
            return `${from} - ${to}`;
        // A single instant
        } else {
            return DateUtils.FormatDate(event.Timestamp, timeFormat);
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
        let e = Object.assign({}, event);
        e.Timestamp = <Date>(<any>event).ActualStart || event.Timestamp;
        this.$emit("eventClicked", e);
    }
}
</script>

<style scoped lang="scss">
.timeline-item {
    cursor: pointer;
}
.timeline-item:hover {
    background-color: #f1eded;
}
.timeline-item-time {
    font-size: 18px;
    min-width: 110px;
    @media (max-width: 540px) {
        margin-right: 0 !important;
        min-width: 80px;
        max-width: 80px;
    }
}
.timeline-item-title {
    font-size: 18px;
}
.timeline-item-title-resolved-label {
    color: var(--v-success-base);
    font-size: 14px;
    margin-left: 10px;
    font-weight: normal;
    font-weight: 600;
}
.timeline-item-description {
    font-size: 16px !important;
}
</style>

<style lang="scss">
.event-timeline-component {
    .v-timeline-item__body {
        @media (max-width: 540px) {
            max-width: calc(100% - 45px);
        }
    }
}
</style>
