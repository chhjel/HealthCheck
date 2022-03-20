<!-- src/components/modules/Overview/SiteEventDetailsComponent.vue -->
<template>
    <card-component color="grey lighten-4" flat
        min-width="350px" max-width="800px">
        <toolbar-component dark
            :color="getEventSeverityColor(event.Severity)">
            <icon-component v-text="getEventSeverityIcon(event.Severity)"/>
            <!-- <btn-component icon>
                <icon-component>edit</icon-component>
            </btn-component> -->
            <div>{{ event.Title }}</div>
                        <div class="subheading">
                {{ getEventTimeLine1(event) }}<br />{{ getEventTimeLine2(event) }}
            </div>
            <!-- <btn-component icon>
                <icon-component>favorite</icon-component>
            </btn-component> -->
            <!-- <btn-component icon>
                <icon-component>more_vert</icon-component>
            </btn-component> -->
        </toolbar-component>
        <div primary-title>
            <span>
                {{ event.Description }}

                <div v-if="event.Resolved && event.ResolvedMessage != null && event.ResolvedMessage.length > 0" class="mt-4">
                    <b v-if="event.ResolvedAt != null">Resolved {{ getResolvedAtDateString(event.ResolvedAt) }}: </b>
                    {{ event.ResolvedMessage }}
                </div>

                <div v-if="event.RelatedLinks.length > 0" class="mt-4">
                    <h4>Related links</h4>
                    <ul>
                        <li v-for="(link, linkIndex) in event.RelatedLinks"
                            :key="`event-${event.id}-details-link-${linkIndex}`">
                            <a :href="link.Url">{{link.Text}}</a>
                        </li>
                    </ul>
                </div>

                <div class="caption mt-4">
                    This is a {{ event.Severity.toString().toLowerCase() }}-level event.<br />
                    EventTypeId: {{ event.EventTypeId }}
                </div>

                <!-- DEVELOPER DETAILS -->
                <expansion-panel-component class="mt-4"
                    v-if="event.DeveloperDetails != null && event.DeveloperDetails.length > 0"
                    v-model:value="developerDetailsExpandedState">
                    <template #header><div>Developer details</div></template>
                    <template #content>
                        <code class="pa-4 dev-details-contents">{{ event.DeveloperDetails }}</code>
                    </template>
                </expansion-panel-component>
            </span>
        </div>
        <div>
            <slot name="actions"></slot>
        </div>
    </card-component>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { SiteEventViewModel } from '@generated/Models/Core/SiteEventViewModel';
import LinqUtils from '@util/LinqUtils';
import { SiteEventSeverity } from '@generated/Enums/Core/SiteEventSeverity';
import DateUtils from '@util/DateUtils';

@Options({
    components: {}
})
export default class SiteEventDetailsComponent extends Vue {
    @Prop()
    event!: SiteEventViewModel;

    developerDetailsExpandedState: boolean = false;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
    }

    ////////////////
    //  GETTERS  //
    //////////////

    ////////////////
    //  METHODS  //
    //////////////
    getResolvedAtDateString(date: Date): string {
        let format = 'dd. MMM HH:mm';
        return DateUtils.FormatDate(date, format);
    }

    getEventTimeLine1(event: SiteEventViewModel) : string {
        if (event.Timestamp.getDate() === event.EndTime.getDate()) {
            let dateFormat = 'dd. MMMM';
            return DateUtils.FormatDate(event.Timestamp, dateFormat);
        } else {
            let timeFormat = 'HH:mm';
            let dateFormat = 'dd. MMM';
            return `${DateUtils.FormatDate(event.Timestamp, `${dateFormat} ${timeFormat}`)} -`;
        }
    }

    getEventTimeLine2(event: SiteEventViewModel) : string {
        let timeFormat = 'HH:mm';
        if (event.Timestamp.getDate() === event.EndTime.getDate()) {
            let start = event.Timestamp;
            let end = this.getEventEndDate(event);
            if (end == null) {
                return DateUtils.FormatDate(start, timeFormat);
            } else {
                return `${DateUtils.FormatDate(start, timeFormat)} - ${DateUtils.FormatDate(end, timeFormat)}`;
            }
        } else {
            let dateFormat = 'dd. MMM';
            return `${DateUtils.FormatDate(event.EndTime, `${dateFormat} ${timeFormat}`)}`;
        }
    }
    
    getEventEndDate(event: SiteEventViewModel) : Date | null {
        if (event.Duration > 1) {
            return new Date(event.Timestamp.getTime() + event.Duration * 60000);
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
}
</script>

<style scoped>
.dev-details-contents::before,
.dev-details-contents::after {
    content:'';
}
</style>