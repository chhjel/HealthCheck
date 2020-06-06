<!-- src/components/modules/TestSuite/paremeter_inputs/ParameterInputComponent.vue -->
<template>
    <div>
        <div class="parameter-header" v-if="showInputHeader">
            <div class="parameter-name">{{ parameter.Name }}</div>
            <v-icon small v-if="hasDescription"
                color="gray" class="parameter-help-icon"
                @click="toggleDescription">help</v-icon>
        </div>

        <div v-show="showDescription" class="parameter-description" v-html="parameter.Description"></div>

        <component
            class="parameter-input"
            :parameter="parameter"
            :type="getGenericListInputType(parameter.Type)"
            :isListItem="isListItem"
            :is="getInputComponentNameFromType(parameter.Type)"
            v-on:disableInputHeader="disableInputHeader">
        </component>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestParameterViewModel from  '../../../../models/modules/TestSuite/TestParameterViewModel';
// Parameter input components
import UnknownParameterInputComponent from './UnknownParameterInputComponent.vue';
import ParameterInputTypeInt32Component from './input_types/ParameterInputTypeInt32Component.vue';
import ParameterInputTypeNullableInt32Component from './input_types/ParameterInputTypeNullableInt32Component.vue';
import ParameterInputTypeDecimalComponent from './input_types/ParameterInputTypeDecimalComponent.vue';
import ParameterInputTypeNullableDecimalComponent from './input_types/ParameterInputTypeNullableDecimalComponent.vue';
import ParameterInputTypeSingleComponent from './input_types/ParameterInputTypeSingleComponent.vue';
import ParameterInputTypeNullableSingleComponent from './input_types/ParameterInputTypeNullableSingleComponent.vue';
import ParameterInputTypeDoubleComponent from './input_types/ParameterInputTypeDoubleComponent.vue';
import ParameterInputTypeNullableDoubleComponent from './input_types/ParameterInputTypeNullableDoubleComponent.vue';
import ParameterInputTypeStringComponent from './input_types/ParameterInputTypeStringComponent.vue';
import ParameterInputTypeBooleanComponent from './input_types/ParameterInputTypeBooleanComponent.vue';
import ParameterInputTypeNullableBooleanComponent from './input_types/ParameterInputTypeNullableBooleanComponent.vue';
import ParameterInputTypeDateTimeComponent from './input_types/ParameterInputTypeDateTimeComponent.vue';
import ParameterInputTypeNullableDateTimeComponent from './input_types/ParameterInputTypeNullableDateTimeComponent.vue';
import ParameterInputTypeDateTimeOffsetComponent from './input_types/ParameterInputTypeDateTimeOffsetComponent.vue';
import ParameterInputTypeNullableDateTimeOffsetComponent from './input_types/ParameterInputTypeNullableDateTimeOffsetComponent.vue';
import ParameterInputTypeEnumComponent from './input_types/ParameterInputTypeEnumComponent.vue';
import ParameterInputTypeFlaggedEnumComponent from './input_types/ParameterInputTypeFlaggedEnumComponent.vue';
import ParameterInputTypeGenericListComponent from './input_types/ParameterInputTypeGenericListComponent.vue';
import ParameterInputTypeHttpPostedFileBaseComponent from './input_types/ParameterInputTypeHttpPostedFileBaseComponent.vue';

@Component({
    components: {
      // Parameter input components
      UnknownParameterInputComponent,
      ParameterInputTypeInt32Component,
      ParameterInputTypeNullableInt32Component,
      ParameterInputTypeDecimalComponent,
      ParameterInputTypeNullableDecimalComponent,
      ParameterInputTypeSingleComponent,
      ParameterInputTypeNullableSingleComponent,
      ParameterInputTypeDoubleComponent,
      ParameterInputTypeNullableDoubleComponent,
      ParameterInputTypeStringComponent,
      ParameterInputTypeBooleanComponent,
      ParameterInputTypeNullableBooleanComponent,
      ParameterInputTypeDateTimeComponent,
      ParameterInputTypeNullableDateTimeComponent,
      ParameterInputTypeDateTimeOffsetComponent,
      ParameterInputTypeNullableDateTimeOffsetComponent,
      ParameterInputTypeEnumComponent,
      ParameterInputTypeFlaggedEnumComponent,
      ParameterInputTypeGenericListComponent,
      ParameterInputTypeHttpPostedFileBaseComponent
    }
})
export default class ParameterInputComponent extends Vue {
    @Prop({ required: true })
    parameter!: TestParameterViewModel;

    @Prop({ required: false })
    isListItem!: boolean;

    showInputHeader: boolean = true;
    showDescription: boolean = false;

    mounted(): void {
    }

    getGenericListInputType(typeName: string): string | null
    {
        let genericListPattern = /^List<(\w+)>$/;
        if (genericListPattern.test(typeName)) {
            let match = typeName.match(genericListPattern);
            let itemType: string | null = (match == null) ? null : match[1];
            return itemType;
        }
        return null;
    }
    
    getInputComponentNameFromType(typeName: string): string
    {
        let genericType = this.getGenericListInputType(typeName);
        if (genericType != null) {
            return "ParameterInputTypeGenericListComponent";
        }

        typeName = typeName.replace('<', '').replace('>', '');
        let componentName = `ParameterInputType${typeName}Component`;
        let componentExists = (this.$options!.components![componentName] != undefined);
        return componentExists 
            ? componentName
            : "UnknownParameterInputComponent";
    }

    disableInputHeader(): void {
        this.showInputHeader = false;
    }

    toggleDescription(): void {
        this.showDescription = !this.showDescription;
    }

    get hasDescription(): boolean {
        return this.parameter.Description != null && this.parameter.Description.length > 0;
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