<!-- src/components/modules/Overview/StatusComponent.vue -->
<template>
    <div class="summary" :class="`with-${type}`">
        <icon-component :color="color" class="icon">{{ icon }}</icon-component>
        <span>{{ text }}</span>
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { SiteEventViewModel } from '@generated/Models/Core/SiteEventViewModel';
import LinqUtils from '@util/LinqUtils';
import { SiteEventSeverity } from '@generated/Enums/Core/SiteEventSeverity';

const TYPE_SUCCESS: string = "success";
const TYPE_INFO: string = "info";
const TYPE_WARNING: string = "warning";
const TYPE_ERROR: string = "error";
const TYPE_FATAL: string = "fatal";

@Options({
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

    get color(): string {
        return '#fff';
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
    align-items: center;
    font-size: 17px;
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
    background: var(--color--fatal-base);
}
</style>