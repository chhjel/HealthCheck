<!-- src/components/paremeter_inputs/input_types/ParameterInputTypeNullableBooleanComponent.vue -->
<template>
    <div>
        <v-checkbox
            v-model="checkboxState"
            :indeterminate="this.parameter.Value == null" 
            :label="label"
            @click="setNextState"
            color="secondary"
            class="parameter-checkbox pt-0 mt-2"
        ></v-checkbox>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestParameterViewModel from '../../../../models/TestSuite/TestParameterViewModel';

@Component({
    components: {
    }
})
export default class ParameterInputTypeNullableBooleanComponent extends Vue {
    @Prop({ required: true })
    parameter!: TestParameterViewModel;

    checkboxState: boolean = false;
    
    mounted(): void {
        // this.$emit("disableInputHeader");
        this.updateCheckboxState();
    }

    get label(): string {
        if (this.parameter.Value == null) {
            return "null";
        } else {
            return (this.parameter.Value.toLowerCase() == "true") ? "Yes" : "No";
        }
    }

    setNextState(): void {
        if (this.parameter.Value == null) {
            this.parameter.Value = "true";
        } else if (this.parameter.Value.toLowerCase() == "true") {
            this.parameter.Value = "false";
        } else {
            this.parameter.Value = null
        }
        this.updateCheckboxState();
    }

    updateCheckboxState(): void {
        this.checkboxState = this.parameter.Value != null && this.parameter.Value.toLowerCase() == "true";
    }
}
</script>

<style>
.parameter-checkbox label {
    color: #000 !important;
}
</style>
