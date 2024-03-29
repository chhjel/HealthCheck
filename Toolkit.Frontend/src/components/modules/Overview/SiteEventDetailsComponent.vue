<!-- src/components/modules/Overview/SiteEventDetailsComponent.vue -->
<template>
    <div style="min-width:350px max-width: 800px">
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

        <div class="small-font mt-4">
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
    </div>
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
}
</script>

<style scoped lang="scss">
.dev-details-contents::before,
.dev-details-contents::after {
    content:'';
}
</style>