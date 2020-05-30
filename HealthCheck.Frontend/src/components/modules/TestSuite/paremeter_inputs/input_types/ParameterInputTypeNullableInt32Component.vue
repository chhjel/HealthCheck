<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeNullableInt32Component.vue -->
<template>
    <div>
        <v-text-field
            type="number"
            class="pt-0"
            v-model="parameter.Value"
            v-on:change="onValueChanged"
            :placeholder="placeholderText"
            required />
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestParameterViewModel from  '../../../../../models/modules/TestSuite/TestParameterViewModel';

@Component({
    components: {
    }
})
export default class ParameterInputTypeNullableInt32Component extends Vue {
    @Prop({ required: true })
    parameter!: TestParameterViewModel;
    
    mounted(): void {
        if (this.parameter.Value === '') {
            this.parameter.Value = '0';
        }
    }

    onValueChanged(): void {
        if (this.parameter.Value === '') {
            this.parameter.Value = null;
        }
    }
    
    get placeholderText(): string {
        return this.parameter.Value == null ? "null" : "";
    }
}
</script>

<style scoped>
</style>
