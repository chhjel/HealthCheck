<!-- src/components/Dataflow/EntryProperties/DataflowEntryPropertyValueComponent.vue -->
<template>
    <div>
        <component
            class="dataflow-entry-property"
            :type="type"
            :raw="raw"
            :title="title"
            :is="getParameterComponentNameFromType()">
        </component>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestParameterViewModel from '../../../models/TestSuite/TestParameterViewModel';
// Parameter input components
import UnknownDataflowEntryPropertyValueComponent from './UnknownDataflowEntryPropertyValueComponent.vue';
import DataflowEntryPropertyValueRawComponent from './DataflowEntryPropertyValueRawComponent.vue';
import DataflowEntryPropertyValueListComponent from './DataflowEntryPropertyValueListComponent.vue';
import DataflowEntryPropertyValueDictionaryComponent from './DataflowEntryPropertyValueDictionaryComponent.vue';
import DataflowEntryPropertyValueLinkComponent from './DataflowEntryPropertyValueLinkComponent.vue';
import DataflowEntryPropertyValueImageComponent from './DataflowEntryPropertyValueImageComponent.vue';
import { DataFlowPropertyUIHint } from "../../../models/Dataflow/DataFlowPropertyDisplayInfo";

@Component({
    components: {
      // Parameter input components
      UnknownDataflowEntryPropertyValueComponent,
      DataflowEntryPropertyValueRawComponent,
      DataflowEntryPropertyValueListComponent,
      DataflowEntryPropertyValueDictionaryComponent,
      DataflowEntryPropertyValueLinkComponent,
      DataflowEntryPropertyValueImageComponent
    }
})
export default class DataflowEntryPropertyValueComponent extends Vue {
    @Prop({ required: true })
    type!: DataFlowPropertyUIHint;

    @Prop({ required: true })
    title!: string;

    @Prop({ required: true })
    raw!: any;

    mounted(): void {
    }

    getParameterComponentNameFromType(): string
    {
        let targetType = this.type;
        if (targetType == DataFlowPropertyUIHint.DateTime)
        {
            targetType = DataFlowPropertyUIHint.Raw;
        }

        let componentName = `DataflowEntryPropertyValue${targetType}Component`;
        let componentExists = (this.$options!.components![componentName] != undefined);
        return componentExists 
            ? componentName
            : "UnknownDataflowEntryPropertyValueComponent";
    }
}
</script>

<style scoped>
.parameter-header {
    text-align: left;
}
.parameter-name {
    display: inline-block;
    font-size: 16px;
    color: var(--v-secondary-base);
    font-weight: 600;
}
.parameter-description {
    text-align: left;
    padding: 10px;
    border-radius: 10px;
    background-color: #ebf1fb;
}
.parameter-help-icon {
    user-select: none;
    font-size: 20px !important;
}
.parameter-help-icon:hover {
    color: #1976d2;
}
</style>

<style>
.parameter-input input {
    font-size: 18px;
    color: #000 !important;
}
</style>