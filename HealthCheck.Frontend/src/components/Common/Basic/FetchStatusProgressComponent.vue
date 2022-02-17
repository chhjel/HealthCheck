<!-- src/components/Common/Basic/FetchStatusProgressComponent.vue -->
<template>
    <div class="fetch-status-progress-component" :style="rootStyle">
        <v-progress-linear
            v-if="visible"
            v-model="progress"
            :height="height"
            :color="color"
        />
    </div>
</template>

<script lang="ts">
import { FetchStatusWithProgress } from "services/abstractions/HCServiceBase";
import { Vue, Component, Prop, Watch } from "vue-property-decorator";

@Component({
    components: {}
})
export default class FetchStatusProgressComponent extends Vue
{
    @Prop({ required: true })
    status!: FetchStatusWithProgress | null;

    @Prop({ required: false, default: 4 })
    height!: number;

    @Prop({ required: false, default: 'primary' })
    color!: string;

    mounted(): void {
    }

    get progress(): number {
        if (!this.status) return 0;
        return this.status.progress; 
    }
    
    get visible(): boolean {
        if (!this.status) return false;
        return this.status.inProgress || this.status.progress > 0;
    }

    get rootStyle(): any {
        return {
            height: `${this.height}px`
        }
    }
}
</script>

<style lang="scss">
.fetch-status-progress-component {
    .v-progress-linear {
        margin: 0;
    }
}
</style>