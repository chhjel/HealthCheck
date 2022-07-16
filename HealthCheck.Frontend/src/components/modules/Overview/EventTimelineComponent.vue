<!-- src/components/modules/Overview/EventTimelineComponent.vue -->
<template>
    <div class="root event-timeline-component">
        <div v-if="timelineEventGroups.length == 0" class="pa-3 info">
            <strong>No recent events</strong>
        </div>

        <div v-for="group in timelineEventGroups"
            :key="`timeline-group-${group.index}-header`"
            class="event-timeline-item-group">
            <div class="event-timeline-item-group__header with-timeline">
                <span>{{group.title}}</span>
            </div>

            <div v-for="event in group.events"
                :key="`timeline-group-${group.index}-item-${event.Id}`"
                class="event-timeline-item with-timeline"
                @click="onEventClicked(event)">
                <div class="event-timeline-item__dot" :class="getTimelineItemDotClasses(event)"></div>
                <div class="event-timeline-item__content">
                    <div class="event-timeline-item__time">
                        <strong>{{getTimelineItemTimeString(event, group)}}</strong>
                    </div>
                    <div class="event-timeline-item__data">
                        <strong class="event-timeline-item__title">
                            {{ event.Title }}
                            <strong class="event-timeline-item__title-resolved-label" v-if="event.Resolved">
                                Resolved
                            </strong>
                        </strong>
                        <div class="caption event-timeline-item__description">{{ event.Description }}</div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { SiteEventViewModel } from '@generated/Models/Core/SiteEventViewModel';
import LinqUtils from '@util/LinqUtils';
import { SiteEventSeverity } from '@generated/Enums/Core/SiteEventSeverity';
import DateUtils from '@util/DateUtils';

interface TimelineGroup {
    index: number;
    title: string;
    date: Date;
    events: Array<SiteEventViewModel>;
}

@Options({
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

    getTimelineItemDotClasses(event: SiteEventViewModel): any {
        let classes: any = {};

        if (event.Severity == SiteEventSeverity.Warning) classes["warning"] = true;
        else if (event.Severity == SiteEventSeverity.Error) classes["error"] = true;
        else if (event.Severity == SiteEventSeverity.Fatal) classes["fatal"] = true;
        else classes["info"] = true;

        return classes;
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
.with-timeline {
    padding-left: 25px;
    border-left: 2px solid #c9c9c9;
    margin-left: 25px;
}
.event-timeline-item-group {
    &__header {
        padding-top: 10px;
        font-variant: all-small-caps;
        color: var(--color--accent-darken8);
        font-size: 20px;
        font-weight: 700;
    }
}
.event-timeline-item {
    cursor: pointer;
    display: flex;
    flex-wrap: nowrap;
    padding-top: 5px;
    padding-bottom: 10px;
    transition: all 0.2s;

    &:hover {
        background-color: #dfdfdf;
    }

    &__dot {
        box-sizing: border-box;
        width: 20px;
        height: 20px;
        min-width: 20px;
        min-height: 20px;
        margin-left: calc(-25px - 10px);
        border-radius: 50%;
        border: 3px solid #fff;
    }

    &__content {
        margin-left: 16px;
        display: flex;
        @media (max-width: 540px) {
            max-width: calc(100% - 60px);
        }
    }

    &__time {
        font-size: 18px;
        min-width: 110px;
        @media (max-width: 540px) {
            margin-right: 0 !important;
            min-width: 80px;
            max-width: 80px;
        }
    }
    &__data {
        margin-left: 10px;
    }
    &__title {
        font-size: 18px;
        display: flex;
        align-items: baseline;
    }
    &__title-resolved-label {
        color: var(--color--success-base);
        font-size: 14px;
        margin-left: 10px;
        font-weight: normal;
        font-weight: 600;
    }
    &__description {
        font-size: 16px !important;
    }
}
</style>
