<!-- src/components/modules/Dataflow/EntryProperties/DataflowEntryPropertyValueComponent.vue -->
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
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { TestParameterViewModel } from '@generated/Models/Core/TestParameterViewModel';
// Parameter input components
import UnknownDataflowEntryPropertyValueComponent from '@components/modules/Dataflow/EntryProperties/UnknownDataflowEntryPropertyValueComponent.vue';
import DataflowEntryPropertyValueRawComponent from '@components/modules/Dataflow/EntryProperties/DataflowEntryPropertyValueRawComponent.vue';
import DataflowEntryPropertyValueListComponent from '@components/modules/Dataflow/EntryProperties/DataflowEntryPropertyValueListComponent.vue';
import DataflowEntryPropertyValueDictionaryComponent from '@components/modules/Dataflow/EntryProperties/DataflowEntryPropertyValueDictionaryComponent.vue';
import DataflowEntryPropertyValueLinkComponent from '@components/modules/Dataflow/EntryProperties/DataflowEntryPropertyValueLinkComponent.vue';
import DataflowEntryPropertyValueImageComponent from '@components/modules/Dataflow/EntryProperties/DataflowEntryPropertyValueImageComponent.vue';
import DataflowEntryPropertyValuePreformattedComponent from '@components/modules/Dataflow/EntryProperties/DataflowEntryPropertyValuePreformattedComponent.vue';
import DataflowEntryPropertyValueHTMLComponent from '@components/modules/Dataflow/EntryProperties/DataflowEntryPropertyValueHTMLComponent.vue';
import DataflowEntryPropertyValueIconComponent from '@components/modules/Dataflow/EntryProperties/DataflowEntryPropertyValueIconComponent.vue';
import { DataFlowPropertyUIHint } from '@generated/Enums/Core/DataFlowPropertyUIHint';

@Options({
    components: {
      // Parameter input components
      UnknownDataflowEntryPropertyValueComponent,
      DataflowEntryPropertyValueRawComponent,
      DataflowEntryPropertyValueListComponent,
      DataflowEntryPropertyValueDictionaryComponent,
      DataflowEntryPropertyValueLinkComponent,
      DataflowEntryPropertyValueImageComponent,
      DataflowEntryPropertyValuePreformattedComponent,
      DataflowEntryPropertyValueHTMLComponent,
      DataflowEntryPropertyValueIconComponent
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
    color: var(--color--secondary-base);
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