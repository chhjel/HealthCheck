<!-- src/components/paremeter_inputs/input_types/ParameterInputTypeBooleanComponent.vue -->
<template>
    <div>
        <v-switch 
            v-model="value" 
            :label="label"
            v-on:change="onChanged"
            color="secondary"
            class="parameter-checkbox pt-0 mt-2"
        ></v-switch>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestParameterViewModel from '../../../../models/TestSuite/TestParameterViewModel';

@Component({
    components: {
    }
})
export default class ParameterInputTypeBooleanComponent extends Vue {
    @Prop({ required: true })
    parameter!: TestParameterViewModel;
    
    value: boolean = false;

    mounted(): void {
        // this.$emit("disableInputHeader");
        this.value = (this.parameter.Value != null && this.parameter.Value.toLowerCase() == "true");
        this.onChanged();
    }

    get label(): string {
        return (this.parameter.Value != null && this.parameter.Value.toLowerCase() == "true") ? "Yes" : "No";
    }

    onChanged(): void {
        this.parameter.Value = this.value.toString();
    }
}
</script>

<style>
.parameter-checkbox label {
    color: #000 !important;
}
</style>
