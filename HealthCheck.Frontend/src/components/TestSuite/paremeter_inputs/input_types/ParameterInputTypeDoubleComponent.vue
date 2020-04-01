<!-- src/components/paremeter_inputs/input_types/ParameterInputTypeDoubleComponent.vue -->
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
export default class ParameterInputTypeDoubleComponent extends Vue {
    @Prop({ required: true })
    parameter!: TestParameterViewModel;
    
    mounted(): void {
        this.validate();
    }

    onValueChanged(): void {
        this.validate();
    }

    validate(): void {
        if (this.parameter.Value == null || this.parameter.Value === '') {
            this.parameter.Value = '0';
        }
    }
}
</script>

<style scoped>
</style>
