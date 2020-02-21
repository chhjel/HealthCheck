<!-- src/components/Dataflow/EntryProperties/DataflowEntryPropertyValueHTMLComponent.vue -->
<template>
    <div>
        <b>{{ title }}</b>
        <div v-html="raw"></div>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import { DataFlowPropertyUIHint } from "../../../models/Dataflow/DataFlowPropertyDisplayInfo";

@Component({
    components: {
    }
})
export default class DataflowEntryPropertyValueHTMLComponent extends Vue {
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
