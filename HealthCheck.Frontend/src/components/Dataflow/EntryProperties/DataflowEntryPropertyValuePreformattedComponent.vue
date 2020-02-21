<!-- src/components/Dataflow/EntryProperties/DataflowEntryPropertyValuePreformattedComponent.vue -->
<template>
    <div>
        <b>{{ title }}</b>
        <pre>{{ raw }}</pre>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import { DataFlowPropertyUIHint } from "../../../models/Dataflow/DataFlowPropertyDisplayInfo";

@Component({
    components: {
    }
})
export default class DataflowEntryPropertyValuePreformattedComponent extends Vue {
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
