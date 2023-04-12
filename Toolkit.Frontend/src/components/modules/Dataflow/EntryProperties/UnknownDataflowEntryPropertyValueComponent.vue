<!-- src/components/modules/Dataflow/EntryProperties/UnknownDataflowEntryPropertyValueComponent.vue -->
<template>
    <div>
        <b>{{ title }}</b>: {{ raw }}
        <div class="unkown-type-error">Unknown data type '{{type}}'. No component created for this type yet.</div>
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
export default class UnknownDataflowEntryPropertyValueComponent extends Vue {
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
.unkown-type-error {
    padding: 3px;
    color: darkred;
}
</style>
