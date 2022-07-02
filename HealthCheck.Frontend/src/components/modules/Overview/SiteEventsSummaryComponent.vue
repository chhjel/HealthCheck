<!-- src/components/modules/Overview/SiteEventsSummaryComponent.vue -->
<template>
    <div class="summary-list">
        <div v-for="(event, index) in events"
            :key="`current-event-${index}`"
            @click="onEventClicked(event)"
            class="summary-list-item pa-2">
            <div class="flex">
                <icon-component :color="getEventIconColor(event)" class="mr-2">{{getEventIcon(event)}}</icon-component>
                <div>
                    <div class="summary-item-title">{{ event.Title }}</div>
                    <div class="summary-item-description">{{ event.Description }}</div>
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


@Options({
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
            return "fatal";
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
    background: var(--color--success-base);
    white-space: pre;
}
.summary.with-info {
    color: #fff;
    background: var(--color--info-base);
}
.summary.with-warning {
    color: #fff;
    background: var(--color--warning-base);
}
.summary.with-error {
    color: #fff;
    background: var(--color--error-base);
}
.summary.with-fatal {
    color: #fff;
    /* background: #333; */
}
.summary-item-title {
    font-size: 18px;
}
.summary-item-description {
    font-size: 16px;
}
.summary-list-item {
    background: #fff;
    margin-bottom: 5px;
    margin-top: 5px;
    cursor: pointer;
}
.summary-list {
    background: inherit;
}
</style>
<style>
.summary-list-item a {
    height: 64px;
}
</style>
