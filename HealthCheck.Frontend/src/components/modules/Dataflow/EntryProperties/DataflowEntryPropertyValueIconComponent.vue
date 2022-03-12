<!-- src/components/modules/Dataflow/EntryProperties/DataflowEntryPropertyValueIconComponent.vue -->
<template>
    <div>
        Icon:
        <v-icon>{{ raw }}</v-icon>
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { DataFlowPropertyUIHint } from '@generated/Enums/Core/DataFlowPropertyUIHint';

@Options({
    components: {
    }
})
export default class DataflowEntryPropertyValueIconComponent extends Vue {
    @Prop({ required: true })
    type!: DataFlowPropertyUIHint;

    @Prop({ required: true })
    title!: string;

    @Prop({ required: true })
    raw!: any;

    mounted(): void {
    }
}
</script>

<style scoped>
</style>
