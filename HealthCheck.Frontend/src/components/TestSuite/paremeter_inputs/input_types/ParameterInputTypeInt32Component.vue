<!-- src/components/paremeter_inputs/input_types/ParameterInputTypeInt32Component.vue -->
<template>
    <div>
        <v-text-field
            type="number"
            class="pt-0"
            v-model="parameter.Value"
            v-on:change="onValueChanged"
            required />
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestParameterViewModel from '../../../../models/TestSuite/TestParameterViewModel';

@Component({
    components: {
    }
})
export default class ParameterInputTypeInt32Component extends Vue {
    @Prop({ required: true })
    parameter!: TestParameterViewModel;
    
    mounted(): void {
        if (this.parameter.Value == null || this.parameter.Value === '') {
            this.parameter.Value = '0';
        }
    }

    onValueChanged(): void {
        if (this.parameter.Value == null || this.parameter.Value === '') {
            this.parameter.Value = "0";
        }
    }
}
</script>

<style scoped>
</style>
