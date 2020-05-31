<!-- src/components/modules/Dataflow/EntryProperties/DataflowEntryPropertyValueRawComponent.vue -->
<template>
    <div>
        <b>{{ title }}</b>: {{ raw }}
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import { DataFlowPropertyUIHint } from  '../../../../models/modules/Dataflow/DataFlowPropertyDisplayInfo';

@Component({
    components: {
    }
})
export default class DataflowEntryPropertyValueRawComponent extends Vue {
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
