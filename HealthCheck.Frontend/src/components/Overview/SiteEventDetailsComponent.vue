<!-- src/components/Overview/SiteEventDetailsComponent.vue -->
<template>
    <v-card color="grey lighten-4" flat
        min-width="350px" max-width="800px">
        <v-toolbar dark
            :color="getEventSeverityColor(event.Severity)">
            <v-icon v-text="getEventSeverityIcon(event.Severity)"/>
            <!-- <v-btn icon>
                <v-icon>edit</v-icon>
            </v-btn> -->
            <v-toolbar-title>{{ event.Title }}</v-toolbar-title>
            <v-spacer></v-spacer>
            <v-toolbar-title class="subheading">
                {{ getEventTimeLine1(event) }}<br />{{ getEventTimeLine2(event) }}
            </v-toolbar-title>
            <!-- <v-btn icon>
                <v-icon>favorite</v-icon>
            </v-btn> -->
            <!-- <v-btn icon>
                <v-icon>more_vert</v-icon>
            </v-btn> -->
        </v-toolbar>
        <v-card-title primary-title>
            <span>
                {{ event.Description }}

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
            </span>
        </v-card-title>
        <v-card-actions>
            <slot name="actions"></slot>
        </v-card-actions>
    </v-card>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import SiteEventViewModel from '../../models/SiteEvents/SiteEventViewModel';
import LinqUtils from "../../util/LinqUtils";
import { SiteEventSeverity } from "../../models/SiteEvents/SiteEventSeverity";
import DateUtils from "../../util/DateUtils";

@Component({
    components: {}
})
export default class SiteEventDetailsComponent extends Vue {
    @Prop()
    event!: SiteEventViewModel;

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
</style>