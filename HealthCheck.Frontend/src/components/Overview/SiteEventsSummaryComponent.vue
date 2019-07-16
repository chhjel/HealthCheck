<!-- src/components/Overview/SiteEventsSummaryComponent.vue -->
<template>
    <v-list>
        <v-list-tile v-for="(event, index) in events"
            :key="`current-event-${index}`"
            @click="onEventClicked(event)"
            class="summary-list-item">
            <v-list-tile-action>
                <v-icon :color="getEventIconColor(event)">{{getEventIcon(event)}}</v-icon>
            </v-list-tile-action>

            <v-list-tile-content>
                <v-list-tile-title>{{ event.Title }}</v-list-tile-title>
                <v-list-tile-sub-title>{{ event.Description }}</v-list-tile-sub-title>
            </v-list-tile-content>
        </v-list-tile>
    </v-list>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import SiteEventViewModel from '../../models/SiteEvents/SiteEventViewModel';
import LinqUtils from "../../util/LinqUtils";
import { SiteEventSeverity } from "../../models/SiteEvents/SiteEventSeverity";


@Component({
    components: {}
})
export default class SiteEventsSummaryComponent extends Vue {
    @Prop()
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

    ////////////////
    //  METHODS  //
    //////////////
    getEventIcon(event: SiteEventViewModel) : string {
        if (event.Severity == SiteEventSeverity.Information) {
            return "info";
        } else if (event.Severity == SiteEventSeverity.Warning) {
            return "warning";
        } else if (event.Severity == SiteEventSeverity.Error) {
            return "error";
        } else if (event.Severity == SiteEventSeverity.Fatal) {
            return "error";
        } else {
            return "done";
        }
    }

    getEventIconColor(event: SiteEventViewModel) : string {
        if (event.Severity == SiteEventSeverity.Information) {
            return "info";
        } else if (event.Severity == SiteEventSeverity.Warning) {
            return "warning";
        } else if (event.Severity == SiteEventSeverity.Error) {
            return "error";
        } else if (event.Severity == SiteEventSeverity.Fatal) {
            return "black";
        } else {
            return "success";
        }
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
.icon {
    margin-right: 5px;
}
.summary {
    padding: 10px;
    display: flex;
    align-items: center;
    border-radius: 2px;
    color: #fff;
    background: var(--v-success-base);
    white-space: pre;
}
.summary.with-info {
    color: #fff;
    background: var(--v-info-base);
}
.summary.with-warning {
    color: #fff;
    background: var(--v-warning-base);
}
.summary.with-error {
    color: #fff;
    background: var(--v-error-base);
}
.summary.with-fatal {
    color: #fff;
    background: #333;
}
</style>
<style>
.summary-list-item a {
    height: 52px;
}
</style>
