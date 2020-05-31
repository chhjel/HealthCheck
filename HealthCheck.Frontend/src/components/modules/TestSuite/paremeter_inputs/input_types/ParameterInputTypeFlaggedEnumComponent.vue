<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeFlaggedEnumComponent.vue -->
<template>
    <div>
        <parameter-input-type-enum-component
            :parameter="parameter"
            :multiple="true" />
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestParameterViewModel from  '../../../../../models/modules/TestSuite/TestParameterViewModel';
import ParameterInputTypeEnumComponent from './ParameterInputTypeEnumComponent.vue';

@Component({
    components: {
        ParameterInputTypeEnumComponent
    }
})
export default class ParameterInputTypeFlaggedEnumComponent extends Vue {
    @Prop({ required: true })
    parameter!: TestParameterViewModel;
}
</script>

<style>
</style>
