<!-- src/components/paremeter_inputs/ParameterInputComponent.vue -->
<template>
    <div>
        <div class="parameter-header" v-if="showInputHeader">
            <div class="parameter-name">{{ parameter.Name }}</div>
            <v-icon small v-if="hasDescription"
                color="gray" class="parameter-help-icon"
                @click="toggleDescription">help</v-icon>
        </div>

        <!-- <v-expand-transition> -->
        <div v-show="showDescription" class="parameter-description">
            {{ parameter.Description }}
        </div>

        <component
            class="parameter-input"
            :parameter="parameter"
            :is="getInputComponentNameFromType(parameter.Type)"
            v-on:disableInputHeader="disableInputHeader">
        </component>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestParameterViewModel from '../../models/TestParameterViewModel';
// Parameter input components
import UnknownParameterInputComponent from './UnknownParameterInputComponent.vue';
import ParameterInputTypeInt32Component from './input_types/ParameterInputTypeInt32Component.vue';
import ParameterInputTypeStringComponent from './input_types/ParameterInputTypeStringComponent.vue';
import ParameterInputTypeBooleanComponent from './input_types/ParameterInputTypeBooleanComponent.vue';

@Component({
    components: {
      // Parameter input components
      UnknownParameterInputComponent,
      ParameterInputTypeInt32Component,
      ParameterInputTypeStringComponent,
      ParameterInputTypeBooleanComponent
    }
})
export default class ParameterInputComponent extends Vue {
    @Prop({ required: true })
    parameter!: TestParameterViewModel;

    showInputHeader: boolean = true;
    showDescription: boolean = false;

    mounted(): void {
    }
    
    getInputComponentNameFromType(typeName: string): string
    {
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
    color: #000;
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