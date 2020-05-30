<!-- src/components/modules/Overview/StatusComponent.vue -->
<template>
    <div class="summary" :class="`with-${type}`">
        <v-icon color="white" class="icon">{{ icon }}</v-icon>
        <span>{{ text }}</span>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import SiteEventViewModel from  '../../../models/modules/SiteEvents/SiteEventViewModel';
import LinqUtils from  '../../../util/LinqUtils';
import { SiteEventSeverity } from  '../../../models/modules/SiteEvents/SiteEventSeverity';

const TYPE_SUCCESS: string = "success";
const TYPE_INFO: string = "info";
const TYPE_WARNING: string = "warning";
const TYPE_ERROR: string = "error";
const TYPE_FATAL: string = "fatal";

@Component({
    components: {}
})
export default class StatusComponent extends Vue {
    @Prop()
    type!: string;
    @Prop({ required: true })
    text!: string;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get icon(): string {
        if (this.type == TYPE_INFO) {
            return "info";
        } else if (this.type == TYPE_WARNING) {
            return "warning";
        } else if (this.type == TYPE_ERROR) {
            return "error";
        } else if (this.type == TYPE_FATAL) {
            return "error";
        } else {
            return "done";
        }
    }

    ////////////////
    //  METHODS  //
    //////////////
}
</script>

<style scoped>
.icon {
    margin-right: 12px;
    /* margin-left: 6px; */
}
.summary {
    padding: 16px;
    display: flex;
    font-size: 17px;
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